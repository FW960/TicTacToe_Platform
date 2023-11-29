using System.Collections.Concurrent;
using Grpc.Core;
using TicTacToe_Platform.Helpers;
using TicTacToe_Platform.Models.Games;
using TicTacToe_Platform.Models.UserManagementModels;

namespace TicTacToe_Platform.Controllers.Grpc;

public class GameSessionManager : TicTacToe_Platform.GameSessionManager.GameSessionManagerBase
{
    private readonly ConcurrentDictionary<string, GameSession> _gameSessions;
    private readonly GamesUtility _gamesUtility;

    public GameSessionManager(ConcurrentDictionary<string, GameSession> gameSessions, GamesUtility gamesUtility)
    {
        _gameSessions = gameSessions;
        _gamesUtility = gamesUtility;
    }

    /*public override async Task GameTurn(IAsyncStreamReader<GameSessionRequest> requestStream,
        IServerStreamWriter<GameSessionResponse> responseStream, ServerCallContext context)
    {
        var gameTurn = requestStream.Current;

        if (!_gameSessions.TryGetValue(gameTurn.GameId, out var gameSession))
            return;

        if (!gameSession.PlayersConnectionInfos.TryGetValue(gameTurn.UserId, out var mainUserConnectionInfo))
            return;

        mainUserConnectionInfo.WebSocket = requestStream;
        mainUserConnectionInfo.responseStream = responseStream;

        var mainUserGameInfo = mainUserConnectionInfo.UserGameInfo;

        UserConnectionInfo? secondUserConnectionInfo = default;
        UserGameInfo? secondUserGameInfo = default;

        var timeoutCancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(60));
        var combinedCancellationToken = CancellationTokenSource
            .CreateLinkedTokenSource(context.CancellationToken, timeoutCancellationTokenSource.Token).Token;

        while (await requestStream.MoveNext(combinedCancellationToken))
        {
            if (gameSession is null)
            {
                return;
            }

            if (gameSession.Game.GameStatus is not (GameStatus.PlayerDisconnected or GameStatus.Draw
                    or GameStatus.PlayerWon or GameStatus.Started) && gameSession.PlayersConnectionInfos.Count == 2)
            {
                gameSession.Game.GameStatus = GameStatus.Started;
                gameSession.Game.StartTime = DateTime.UtcNow;
                
                _gamesUtility.StartGame(gameSession.Game.Id, GameStatus.Started, gameSession.Game.StartTime);
            }

            gameTurn = requestStream.Current;

            if (secondUserConnectionInfo is null)
            {
                secondUserConnectionInfo =
                    gameSession.PlayersConnectionInfos.First(x => !x.Value.UserGameInfo.Id.Equals(mainUserGameInfo.Id))
                        .Value;

                secondUserGameInfo = secondUserConnectionInfo.UserGameInfo;
            }

            if (!gameSession.CurrentUserIdTurn.Equals(mainUserGameInfo.UserId)) continue;
            

            var result = gameSession.Game.ApplyGameTurn(gameTurn.X, gameTurn.Y, mainUserGameInfo.IsZeroes);

            _gamesUtility.CreateTurn(gameSession.Game.Id, mainUserGameInfo.UserId, gameTurn.X, gameTurn.Y,
                mainUserGameInfo.IsZeroes, result, gameSession.Game.Turns.Count + 1,
                gameSession.Game.Turns.LastOrDefault()?.Id ?? "", out var turn);

            gameSession.Game.Turns.Add(turn);

            gameSession.CurrentUserIdTurn = gameSession.PlayersConnectionInfos
                .First(x => !x.Value.UserGameInfo.UserId.Equals(gameSession.CurrentUserIdTurn)).Value.UserGameInfo
                .UserId;

            switch (turn.TurnResult)
            {
                case TurnResult.PlayerWon:
                    mainUserGameInfo.UserGameResult = UserGameResult.PlayerWon;
                    secondUserGameInfo.UserGameResult = UserGameResult.Lost;
                    gameSession.Game.GameStatus = GameStatus.PlayerWon;
                    break;
                case TurnResult.Draw:
                    mainUserGameInfo.UserGameResult = UserGameResult.Draw;
                    secondUserGameInfo.UserGameResult = UserGameResult.Draw;
                    gameSession.Game.GameStatus = GameStatus.Draw;
                    break;
            }

            foreach (var connection in gameSession.PlayersConnectionInfos.Values)
            {
                try
                {
                    await connection.responseStream.WriteAsync(new GameSessionResponse
                    {
                        TurnResult = Convert.ToInt32(turn.TurnResult),
                        X = turn.XPlace.Value,
                        Y = turn.YPlace.Value,
                        YourTurn = gameSession.CurrentUserIdTurn.Equals(mainUserGameInfo.UserId)
                    });
                }
                catch
                {
                    connection.UserGameInfo.UserGameResult = UserGameResult.PlayerDisconnected;
                    gameSession.Game.GameStatus = GameStatus.PlayerDisconnected;
                }
            }

            if (gameSession.Game.GameStatus is not (GameStatus.Started or GameStatus.Created))
            {
                gameSession.Game.EndTime = DateTime.UtcNow;
                
                _gamesUtility.EndGame(gameSession.Game.Id, gameSession.Game.GameStatus, gameSession.Game.EndTime);

                foreach (var connection in gameSession.PlayersConnectionInfos.Values)
                {
                    _gamesUtility.UpdateUserGameInfoStatus(connection.UserGameInfo.UserId,
                        connection.UserGameInfo.UserGameResult);
                }

                _gameSessions.Remove(gameSession.Game.Id, out gameSession);
                gameSession = null;
            }
        }
    }*/
}