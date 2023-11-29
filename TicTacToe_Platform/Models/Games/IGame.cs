namespace TicTacToe_Platform.Models.Games;

public interface IGame
{
    public string GameName { get; set; }
    
    public string GamePassword { get; set; }
}