using TicTacToe_Platform.Helpers;

namespace TicTacToe_Platform.Middlewares;

public class UserFilterMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IdentityUtility _identityUtility;

    public UserFilterMiddleware(RequestDelegate next, IdentityUtility identityUtility)
    {
        _next = next;
        _identityUtility = identityUtility;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        /*if (context.Request.Path.Equals("GameSession"))
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                await _gameSessionHandler.HandleGameSession(webSocket);
            }
            else
            {
                context.Response.StatusCode = 400;
            }
        }*/
        /*else
        {*/
        if (context.Request.Method == "GET" && !context.Request.Path.Value.Contains("assets"))
        {
            var path = context.Request.Path.Value;

            if (path.Contains("login", StringComparison.OrdinalIgnoreCase) ||
                path.Contains("signup", StringComparison.Ordinal) ||
                path.Equals("/"))
            {
                if (_identityUtility.HaveToken(context))
                {
                    context.Response.Redirect("/games");
                }

                if (path.Equals("/"))
                {
                    context.Response.Redirect("/login");
                    return;
                }
            }
        }


        await _next(context);
        /*}*/
    }
}