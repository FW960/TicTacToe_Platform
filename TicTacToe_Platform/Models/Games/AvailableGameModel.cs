using System.ComponentModel.DataAnnotations.Schema;

namespace TicTacToe_Platform.Models.Games;

[Table("Games")]
public class AvailableGameModel : IGame
{
    public string Id { get; set; }

    public string GameName { get; set; }
    public string CreatorId { get; set; }
    public string GamePassword { get; set; }
}