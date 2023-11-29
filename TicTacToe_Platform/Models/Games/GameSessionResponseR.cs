namespace TicTacToe_Platform.Models.Games;

public class GameSessionResponseR
{
    public TurnResult TurnResult { get; set; }
    public string CurrentUserIdTurn { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
}