using TicTacToe_Platform.Middlewares;
using TicTacToe_Platform.Models.Authentication;
using TicTacToe_Platform.Models.UserManagementModels;

namespace TicTacToe_Platform.Helpers;

public class IdentityUtility
{
    public const string AuthorizationTokenName = "X-AUTHORIZATION-TOKEN";
    private readonly CryptoUtility _cryptoUtility;

    public IdentityUtility(CryptoUtility cryptoUtility)
    {
        _cryptoUtility = cryptoUtility;
    }

    public void Login(HttpContext context, User user)
    {
        var authorizedUserInfo = new AuthorizedUserTokenInfo
        {
            User = user,
            UtcCreateTime = DateTime.UtcNow
        };

        var token = _cryptoUtility.EncryptObject(authorizedUserInfo);

        context.Response.Cookies.Append(AuthorizationTokenName, token);
    }

    public bool TryGetTokenInfo(HttpContext context, out AuthorizedUserTokenInfo? tokenInfo)
    {
        if (context.Request.Cookies.TryGetValue(AuthorizationTokenName, out var token))
        {
            tokenInfo = _cryptoUtility.DecryptObject<AuthorizedUserTokenInfo>(token);

            if (tokenInfo is not null)
            {
                return true;
            }
        }

        tokenInfo = null;
        return false;
    }

    public bool HaveToken(HttpContext context)
    {
        return context.Request.Cookies.ContainsKey(AuthorizationTokenName);
    }

    public User? GetCurrentUser(HttpContext context) =>
        context.Items.TryGetValue(AuthenticationHandler.UserItemName, out var token) ? (token as AuthorizedUserTokenInfo).User : null;


    public void LogOut(HttpContext context) => context.Response.Cookies.Delete(AuthorizationTokenName);
}