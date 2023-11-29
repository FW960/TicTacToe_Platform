namespace TicTacToe_Platform.Models.Games;

public class CreateGameModel : IGame
{
    public string GameName { get; set; }
    public string? GamePassword { get; set; }
    public string CreatorId { get; set; }
}