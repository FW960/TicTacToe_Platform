using TicTacToe_Platform.Models.Configurations;
using TicTacToe_Platform.Models.UserManagementModels;

namespace TicTacToe_Platform.Models.Authentication;

public class AuthorizedUserTokenInfo
{
    public DateTime UtcCreateTime { get; set; }
    public User User { get; set; }

    public bool IsValid() => DateTime.UtcNow < UtcCreateTime + AuthorizationConfiguration.TokenLiveTime;
}