using System.ComponentModel.DataAnnotations.Schema;

namespace TicTacToe_Platform.Models.Games;

[Table("UsersGamesInfo")]
public class UserGameInfo
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public string GameId { get; set; }
    public bool IsZeroes { get; set; }
    public DateTime JoinDate { get; set; }
    
    public UserGameResult UserGameResult { get; set; }
}

public enum UserGameResult
{
    PlayerWon,
    Draw,
    PlayerDisconnected,
    Lost,
    NoResult
}