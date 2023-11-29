using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using TicTacToe_Platform.Helpers;
using TicTacToe_Platform.Models.Games;

namespace TicTacToe_Platform.Controllers.Hubs;

public class GameSessionManagerHub : Hub
{
    private readonly ConcurrentDictionary<string, GameSession> _gameSessions;
    private readonly GamesUtility _gamesUtility;
    private const string GameIdItemKey = "oerintf9nf5943";
    private const string UserItemKey = "30i4ftm49045y8";

    public GameSessionManagerHub(ConcurrentDictionary<string, GameSession> gameSessions, GamesUtility gamesUtility)
    {
        _gameSessions = gameSessions;
        _gamesUtility = gamesUtility;
    }

    public async Task JoinGameSession(string gameId, string userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, gameId);
        var group = Clients.Group(gameId);
        await group.SendAsync("JoinGameSessionResponse", "Player have joined");

        var gameSession = _gameSessions[gameId];
        Context.Items.Add(GameIdItemKey, gameId);
        Context.Items.Add(UserItemKey, userId);

        var isZeroes = gameSession.Game.UserGameInfos!.Count == 0
            ? new Random().Next(1, 10) > 5
            : !gameSession.Game.UserGameInfos.First().IsZeroes;

        _gamesUtility.CreateUserGameInfo(userId, gameId, isZeroes, out var userGameInfo);
        gameSession.Game.UserGameInfos.Add(userGameInfo);

        if (gameSession.Game.UserGameInfos.Count == 2)
        {
            _gamesUtility.StartGame(gameSession.Game.Id, GameStatus.Started, DateTime.UtcNow);
            await group.SendAsync("GameSessionStart", gameSession.Game.UserGameInfos.ToArray());
        }
    }

    public async Task HandleGameSessionTurn(string gameSessionRequestJson)
    {
        var gameSessionRequest = JsonConvert.DeserializeObject<GameSessionRequestR>(gameSessionRequestJson);
        var gameSession = _gameSessions[gameSessionRequest.GameId];
        var group = Clients.Group(gameSessionRequest.GameId);

        var gameTurn = gameSession.Game.ApplyGameTurn(gameSessionRequest.X, gameSessionRequest.Y,
            gameSession.Game.UserGameInfos.First(x => x.UserId.Equals(gameSessionRequest.UserId)).IsZeroes,
            gameSessionRequest.UserId);
        _gamesUtility.SaveGameTurn(gameTurn);

        gameSession.CurrentUserIdTurn =
            gameSession.Game.UserGameInfos.First(x => !x.UserId.Equals(gameSessionRequest.UserId)).UserId;

        await group.SendAsync("HandleGameSessionResponse", new GameSessionResponseR
        {
            CurrentUserIdTurn = gameSession.CurrentUserIdTurn,
            X = gameTurn.XPlace!.Value,
            Y = gameTurn.YPlace!.Value,
            TurnResult = gameTurn.TurnResult
        });

        if (gameTurn.TurnResult is not TurnResult.NextTurn)
        {
            foreach (var gameInfo in gameSession.Game.UserGameInfos)
            {
                gameInfo.UserGameResult = gameTurn.TurnResult switch
                {
                    TurnResult.Draw => UserGameResult.Draw,
                    TurnResult.PlayerWon => gameTurn.UserId.Equals(gameInfo.UserId) ? UserGameResult.PlayerWon : UserGameResult.Lost,
                    TurnResult.TurnError => gameTurn.UserId.Equals(gameInfo.UserId) ? UserGameResult.Lost : UserGameResult.PlayerWon
                };

                _gamesUtility.UpdateUserGameInfoStatus(gameInfo.Id, gameInfo.UserGameResult);
            }

            if (gameSession.Game.GameStatus is GameStatus.Draw)
            {
                _gamesUtility.EndGame(gameSession.Game.Id, gameSession.Game.GameStatus, DateTime.UtcNow);
            }
            else
            {
                var userGameInfo = gameSession.Game.UserGameInfos.First(x => x.UserGameResult == UserGameResult.PlayerWon);
                _gamesUtility.EndGame(gameSession.Game.Id, gameSession.Game.GameStatus, DateTime.UtcNow, userGameInfo.UserId);
            }
        }
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var gameId = Context.Items[GameIdItemKey] as string;
        var userId = Context.Items[UserItemKey] as string;

        if (_gameSessions.TryRemove(gameId, out var gameSession) && gameSession.Game.Turns.Count < 9 &&
            gameSession.Game.GameStatus is GameStatus.Started or GameStatus.Created)
        {
            var group = Clients.Group(gameSession.Game.Id);
            await group.SendAsync("HandleGameSessionResponse", new GameSessionResponseR
            {
                TurnResult = TurnResult.PlayerDisconnected,
                X = -1,
                Y = -1
            });

            gameSession.Game.GameStatus = GameStatus.PlayerDisconnected;

            foreach (var userGameInfo in gameSession.Game.UserGameInfos)
            {
                userGameInfo.UserGameResult = userGameInfo.UserId.Equals(userId)
                    ? UserGameResult.PlayerDisconnected
                    : UserGameResult.PlayerWon;

                if (userGameInfo.UserGameResult is UserGameResult.PlayerWon)
                    _gamesUtility.EndGame(gameSession.Game.Id, GameStatus.PlayerDisconnected, DateTime.UtcNow, userGameInfo.UserId);
                
                _gamesUtility.UpdateUserGameInfoStatus(userGameInfo.Id, userGameInfo.UserGameResult);
            }

        }

        await base.OnDisconnectedAsync(exception);
    }
}