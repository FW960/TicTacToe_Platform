using TicTacToe_Platform.Models.UserManagementModels;

namespace TicTacToe_Platform.Models.Games;

public class GameSession
{
    public Game Game { get; set; }
    
    public string CurrentUserIdTurn { get; set; }
}