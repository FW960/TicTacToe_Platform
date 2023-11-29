using Dapper;
using MySql.Data.MySqlClient;
using TicTacToe_Platform.Models.Games;
using TicTacToe_Platform.Models.UserManagementModels;

namespace TicTacToe_Platform.Helpers;

public class GamesUtility
{
    private readonly CryptoUtility _cryptoUtility;
    private readonly MySqlConnection _sqlConnection;
    private readonly MySqlConnection _gameSessionConnection;

    public GamesUtility(MySqlConnection sqlConnection, CryptoUtility cryptoUtility,
        MySqlConnection gameSessionConnection)
    {
        _sqlConnection = sqlConnection;
        _cryptoUtility = cryptoUtility;
        _gameSessionConnection = gameSessionConnection;
        gameSessionConnection.Open();
    }

    public bool CreateGame(CreateGameModel model, out Game game)
    {
        try
        {
            game = new Game
            {
                Id = Guid.NewGuid().ToString(),
                GameName = model.GameName,
                GamePassword = model.GamePassword,
                GameStatus = GameStatus.Created,
                CreateTime = DateTime.UtcNow,
                CreatorId = model.CreatorId,
                UserGameInfos = new List<UserGameInfo>(),
                Turns = new List<GameTurn>()
            };

            var cmd = _sqlConnection.CreateCommand();

            cmd.CommandText =
                @$"INSERT INTO games (Id, GameStatus, GameName {(string.IsNullOrEmpty(game.GamePassword) ? "" : ", GamePassword")},CreateTime, CreatorId) 
                VALUES('{game.Id}', '{Convert.ToInt32(game.GameStatus)}', '{game.GameName}' {(string.IsNullOrEmpty(game.GamePassword) ? "" : $", '{_cryptoUtility.ShaMacEncryptString(game.GamePassword)}'")}, '{game.CreateTime:yyyy-MM-dd H:mm:ss}', '{game.CreatorId}')";

            var result = cmd.ExecuteNonQuery();

            if (result > 0)
            {
                return true;
            }

            game = null;
            return false;
        }
        catch (Exception e)
        {
            //todo logger
            Console.WriteLine(e);
            throw;
        }
    }

    public List<AvailableGameModel> GetAvailableGames()
    {
        try
        {
            var gamesList = _sqlConnection.Query<AvailableGameModel>(
                $"SELECT * FROM GAMES WHERE GameStatus={Convert.ToInt32(GameStatus.Created)} ORDER BY CreateTime desc");

            return gamesList.ToList();
        }
        catch (Exception e)
        {
            //todo logger
            Console.WriteLine(e);
            throw;
        }
    }

    public Dictionary<string, Game> GetUserPlayedGames(string userId)
    {
        try
        {
            /*return new List<Game>();*/
            var gamesTemp = new Dictionary<string, Game>();

            var gamesList = _sqlConnection.Query<Game, UserGameInfo, GameTurn, Game>(
                @$"SELECT
            g.Id, g.GameStatus, g.GameName, g.GamePassword, g.WinnerId, g.CreatorId, g.CreateTime, g.EndTime, g.StartTime,  
            pg.Id, pg.UserId, pg.GameId, pg.IsZeroes, pg.JoinDate, pg.UserGameResult,
            gt.Id, gt.GameId, gt.PrevTurnId, gt.TurnResult, gt.TurnCount, gt.XPlace, gt.YPlace, gt.UserId, gt.TurnEnd, gt.IsZeroes
            FROM GAMES g 
            INNER JOIN UsersGamesInfo pg on pg.GameId = g.Id
            INNER JOIN GamesTurns gt on gt.GameId = g.Id
            WHERE (GameStatus={Convert.ToInt32(GameStatus.Draw)} OR GameStatus={Convert.ToInt32(GameStatus.PlayerDisconnected)} OR GameStatus={Convert.ToInt32(GameStatus.PlayerWon)}) 
            AND pg.UserId='{userId}'
            ORDER BY g.CreateTime desc", (game, userGameInfo, turn) =>
                {
                    if (!gamesTemp.TryGetValue(game.Id, out var currentGame))
                    {
                        currentGame = game;
                        currentGame.Turns = new List<GameTurn>();

                        if (userGameInfo is not null)
                        {
                            if (currentGame.UserGameInfos is null)
                                currentGame.UserGameInfos = new List<UserGameInfo>();
                            
                            currentGame.UserGameInfos.Add(userGameInfo);
                        }
                        
                        gamesTemp.Add(game.Id, currentGame);
                    }

                    if (turn is not null)
                    {
                        currentGame.Turns.Add(turn);
                    }
                    if (currentGame.UserGameInfos.FirstOrDefault(x => x.UserId.Equals(userGameInfo.UserId)) is null)
                    {
                        currentGame.UserGameInfos.Add(userGameInfo);
                    }

                    return currentGame;
                }, splitOn: "Id, Id");

            return gamesTemp;
        }
        catch (Exception e)
        {
            //todo logger
            Console.WriteLine(e);
            throw;
        }
    }

    public bool TryFindGameToJoin(string gameId, string? gamePassword, out Game game)
    {
        try
        {
            game = _sqlConnection
                .Query<Game>(
                    $"SELECT * FROM Games WHERE Id='{gameId}' {(string.IsNullOrEmpty(gamePassword) ? "" : $"AND GamePassword='{_cryptoUtility.ShaMacEncryptString(gamePassword)}'")}")
                .FirstOrDefault();

            return game is not null;
        }
        catch (Exception e)
        {
            //todo logger
            Console.WriteLine(e);
            throw;
        }
    }

    public bool CreateTurn(string gameId, string userId, int x, int y, bool isZeroes, TurnResult turnResult,
        int turnCount,
        string prevTurnId, out GameTurn turn)
    {
        try
        {
            turn = new GameTurn()
            {
                GameId = gameId,
                Id = Guid.NewGuid().ToString(),
                XPlace = x,
                YPlace = y,
                IsZeroes = isZeroes,
                UserId = userId,
                TurnResult = turnResult,
                TurnCount = turnCount,
                PrevTurnId = prevTurnId,
                TurnEnd = DateTime.UtcNow
            };

            var cmd = _gameSessionConnection.CreateCommand();

            cmd.CommandText =
                @$"INSERT INTO GamesTurns (Gameid, PrevTurnId, TurnResult, TurnCount, XPlace, YPlace, UserId, TurnEnd, IsZeroes)
                VALUES ('{turn.GameId}', '{turn.PrevTurnId}', {Convert.ToInt32(turn.TurnResult)}, {turn.TurnCount}, {turn.XPlace}, {turn.YPlace}, '{turn.UserId}', '{turn.TurnEnd:yyyy-MM-dd H:mm:ss}', {Convert.ToInt32(turn.IsZeroes)})";

            var result = cmd.ExecuteNonQuery();

            return result > 0;
        }
        catch (Exception e)
        {
            //todo logger
            Console.WriteLine(e);
            throw;
        }
    }

    public bool CreateUserGameInfo(string userId, string gameId, bool isZeroes, out UserGameInfo userGameInfo)
    {
        try
        {
            userGameInfo = new UserGameInfo
            {
                Id = Guid.NewGuid().ToString(),
                GameId = gameId,
                UserId = userId,
                IsZeroes = isZeroes,
                JoinDate = DateTime.UtcNow,
                UserGameResult = UserGameResult.NoResult
            };

            var cmd = _gameSessionConnection.CreateCommand();

            cmd.CommandText = @$"INSERT INTO UsersGamesInfo (Id, GameId, UserId, IsZeroes, JoinDate, UserGameResult) 
            VALUES ('{userGameInfo.Id}', '{userGameInfo.GameId}', '{userGameInfo.UserId}', {Convert.ToInt32(isZeroes)}, '{userGameInfo.JoinDate:yyyy-MM-dd H:mm:ss}', '{Convert.ToInt32(userGameInfo.UserGameResult)}')";

            var result = cmd.ExecuteNonQuery();

            return result > 0;
        }
        catch (Exception e)
        {
            //todo logger
            Console.WriteLine(e);
            throw;
        }
    }

    public bool UpdateUserGameInfoStatus(string userGameInfoId, UserGameResult userGameInfoStatus)
    {
        try
        {
            var cmd = _gameSessionConnection.CreateCommand();

            cmd.CommandText =
                $"UPDATE UsersGamesInfo SET UserGameResult = {Convert.ToInt32(userGameInfoStatus)} WHERE Id = '{userGameInfoId}'";

            return cmd.ExecuteNonQuery() > 0;
        }
        catch (Exception e)
        {
            //todo logger
            Console.WriteLine(e);
            throw;
        }
    }

    public bool StartGame(string gameId, GameStatus gameStatus, DateTime gameStartTime)
    {
        try
        {
            var cmd = _gameSessionConnection.CreateCommand();

            cmd.CommandText =
                $"UPDATE Games SET GameStatus = {Convert.ToInt32(gameStatus)}, StartTime = '{gameStartTime:yyyy-MM-dd H:mm:ss}' WHERE Id = '{gameId}'";

            return cmd.ExecuteNonQuery() > 0;
        }
        catch (Exception e)
        {
            //todo logger
            Console.WriteLine(e);
            throw;
        }
    }

    public bool EndGame(string gameId, GameStatus gameStatus, DateTime gameEndTime, string? userId = null)
    {
        try
        {
            var cmd = _gameSessionConnection.CreateCommand();

            cmd.CommandText =
                $"UPDATE Games SET GameStatus = {Convert.ToInt32(gameStatus)}, EndTime = '{gameEndTime:yyyy-MM-dd H:mm:ss}'{(string.IsNullOrEmpty(userId) ? "" : $", WinnerId = '{userId}'")} WHERE Id = '{gameId}'";

            return cmd.ExecuteNonQuery() > 0;
        }
        catch (Exception e)
        {
            //todo logger
            Console.WriteLine(e);
            throw;
        }
    }

    public bool SaveGameTurn(GameTurn turn)
    {
        try
        {
            var cmd = _gameSessionConnection.CreateCommand();

            cmd.CommandText = @$"
            INSERT INTO GamesTurns (Id, GameId, PrevTurnId, TurnResult, TurnCount, XPlace, YPlace, UserId, TurnEnd, IsZeroes)
            VALUES ('{turn.Id}', '{turn.GameId}', '{turn.PrevTurnId}', {Convert.ToInt32(turn.TurnResult)}, {turn.TurnCount}, {turn.XPlace}, {turn.YPlace}, '{turn.UserId}', '{turn.TurnEnd:yyyy-MM-dd H:mm:ss}',{Convert.ToInt32(turn.IsZeroes)})";

            return cmd.ExecuteNonQuery() > 0;
        }
        catch (Exception e)
        {
            //todo logger
            Console.WriteLine(e);
            throw;
        }
    }

    public void RemoveGame(string gameId)
    {
        try
        {
            var cmd = $@"
            DELETE FROM Games WHERE Id = '{gameId}';
            DELETE FROM GamesTurns WHERE GameId = '{gameId}';
            DELETE FROM UsersGamesInfo WHERE GameId = '{gameId}';";

            var result = _gameSessionConnection.Execute(cmd);
        }
        catch (Exception e)
        {
            //todo logger
            Console.WriteLine(e);
            throw;
        }
    }
}