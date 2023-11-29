using System.ComponentModel.DataAnnotations.Schema;

namespace TicTacToe_Platform.Models.Games;

[Table("GamesTurns")]
public class GameTurn
{
    public string Id { get; set; }
    
    public string GameId { get; set; }
    
    public string? PrevTurnId { get; set; }
    
    public TurnResult TurnResult { get; set; }
    
    public int TurnCount { get; set; }
    
    public int? YPlace { get; set; }
    
    public int? XPlace { get; set; }
    
    public string UserId { get; set; }
    
    public DateTime TurnEnd { get; set; }
    
    public bool IsZeroes { get; set; }
}

public enum TurnResult
{
    PlayerWon,
    Draw,
    PlayerDisconnected,
    NextTurn,
    TurnError
}