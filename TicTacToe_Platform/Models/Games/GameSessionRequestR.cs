namespace TicTacToe_Platform.Models.Games;

public class GameSessionRequestR
{
    public string GameId { get; set; }
    public string UserId { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
}