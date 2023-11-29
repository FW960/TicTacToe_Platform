using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using TicTacToe_Platform.Helpers;
using TicTacToe_Platform.Models.Authentication;
using TicTacToe_Platform.Models.UserManagementModels;

namespace TicTacToe_Platform.Middlewares;

public class AuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string UserItemName = "AUTHORIZED-USER";
    private readonly CryptoUtility _cryptoUtility;
    private readonly UserUtility _userUtility;
    private readonly IdentityUtility _identityUtility;

    public AuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger,
        UrlEncoder encoder, ISystemClock clock, CryptoUtility cryptoUtility, UserUtility userUtility,
        IdentityUtility identityUtility) : base(
        options, logger, encoder, clock)
    {
        _cryptoUtility = cryptoUtility;
        _userUtility = userUtility;
        _identityUtility = identityUtility;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if ((Request.Path.Value.Contains("game", StringComparison.OrdinalIgnoreCase) ||
            Request.Path.Value.Contains("account", StringComparison.OrdinalIgnoreCase)) && !Request.Path.Value.Contains("assets"))
        {
            if (!_identityUtility.TryGetTokenInfo(Context, out var token))
            {
                return Task.FromResult(AuthenticateResult.Fail("Missing Authorization token"));
            }

            if (token is null || !token.IsValid() || !_userUtility.TryFindUserById(token.User.Id, out var user))
            {
                _identityUtility.LogOut(Context);
                return Task.FromResult(AuthenticateResult.Fail("Incorrect Authorization token"));
            }

            var claims = new[] {new Claim(ClaimTypes.Authentication, "username")};
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            Context.Items.Add(UserItemName, token);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
        return Task.FromResult(AuthenticateResult.NoResult());
    }
    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Context.Response.Redirect("/login");
        return Task.CompletedTask;
    }
    protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
    {
        Response.Redirect("/Login");

        Response.StatusCode = (int)HttpStatusCode.Redirect;
        return Task.CompletedTask;
    }
}