using System.ComponentModel.DataAnnotations.Schema;
using TicTacToe_Platform.Models.UserManagementModels;

namespace TicTacToe_Platform.Models.Games;

[Table("Games")]
public class Game
{
    public string Id { get; set; }

    public GameStatus GameStatus { get; set; }

    public string GameName { get; set; }

    public string? GamePassword { get; set; }

    public string? WinnerId { get; set; }

    public List<GameTurn> Turns { get; set; }

    public DateTime CreateTime { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public List<UserGameInfo>? UserGameInfos { get; set; }
    public string CreatorId { get; set; }

    private string[,]? _gameField { get; set; }

    public string[,] GameField
    {
        get
        {
            if (_gameField is not null)
                return _gameField;

            _gameField = new string[3, 3];

            foreach (var turn in Turns.OrderBy(x => x.TurnCount))
            {
                if (turn.TurnResult is not TurnResult.TurnError)
                    _gameField[turn.XPlace.Value, turn.YPlace.Value] = turn.IsZeroes ? "O" : "X";
            }

            return _gameField;
        }
    }

    public GameTurn ApplyGameTurn(int x, int y, bool isZeroes, string userId)
    {
        var turnResult = TurnResult.NextTurn;

        if (x < 0 || x > 2 || y < 0 || y > 2)
        {
            GameStatus = GameStatus.GameError;
            turnResult = TurnResult.TurnError;
        }

        if (turnResult is not TurnResult.TurnError)
        {
            GameField[x, y] = isZeroes ? "0" : "X";

            if (CheckForWin(x, y))
            {
                GameStatus = GameStatus.PlayerWon;
                turnResult = TurnResult.PlayerWon;
            }

            if (CheckForDraw())
            {
                GameStatus = GameStatus.Draw;
                turnResult = TurnResult.Draw;
            }
        }

        var gameTurn = new GameTurn
        {
            XPlace = x,
            YPlace = y,
            TurnResult = turnResult,
            IsZeroes = isZeroes,
            TurnCount = Turns.Count + 1,
            PrevTurnId = Turns.LastOrDefault()?.Id,
            GameId = Id,
            UserId = userId,
            TurnEnd = DateTime.UtcNow,
            Id = Guid.NewGuid().ToString()
        };
        Turns.Add(gameTurn);

        if (gameTurn.TurnResult is TurnResult.PlayerWon or TurnResult.Draw or TurnResult.TurnError)
        {
            foreach (var userGameInfo in UserGameInfos)
            {
                userGameInfo.UserGameResult = gameTurn.TurnResult switch
                {
                    TurnResult.Draw => UserGameResult.Draw,
                    TurnResult.PlayerWon => userGameInfo.GameId.Equals(userId)
                        ? UserGameResult.Lost
                        : UserGameResult.PlayerWon,
                    TurnResult.TurnError => UserGameResult.NoResult,
                    _ => UserGameResult.NoResult
                };
            }
        }

        return gameTurn;
    }

    private bool CheckForWin(int x, int y)
    {
        // Check row
        if (GameField[x, 0] == GameField[x, 1] && GameField[x, 1] == GameField[x, 2])
        {
            return true;
        }

        // Check column
        if (GameField[0, y] == GameField[1, y] && GameField[1, y] == GameField[2, y])
        {
            return true;
        }

        // Check diagonals
        if (x == y && GameField[0, 0] == GameField[1, 1] && GameField[1, 1] == GameField[2, 2])
        {
            return true;
        }

        if (x + y == 2 && GameField[0, 2] == GameField[1, 1] && GameField[1, 1] == GameField[2, 0])
        {
            return true;
        }

        return false;
    }

    private bool CheckForDraw()
    {
        for (var i = 0; i < 3; i++)
        for (var j = 0; j < 3; j++)
            if (string.IsNullOrEmpty(_gameField[i, j]))
                return false;

        return true;
    }
}

public enum GameStatus
{
    PlayerWon,
    Draw,
    PlayerDisconnected,
    Created,
    Started,
    GameError
}