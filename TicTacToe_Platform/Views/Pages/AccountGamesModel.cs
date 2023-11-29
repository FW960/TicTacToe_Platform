using System.Collections.Concurrent;
using TicTacToe_Platform.Models.Games;
using TicTacToe_Platform.Models.UserManagementModels;

namespace TicTacToe_Platform.Views.Pages;

public class AccountGamesModel
{
    public bool IsGames { get; set; }
    public Dictionary<string, Game> UserPlayedGames { get; set; }
    public ConcurrentDictionary<string, GameSession> AvailableGames { get; set; }
    
    public User User { get; set; }
}