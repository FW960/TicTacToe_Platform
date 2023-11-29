namespace TicTacToe_Platform.Middlewares;

public class HeadersManagerMiddleware
{
    private readonly RequestDelegate _next;

    public HeadersManagerMiddleware(RequestDelegate next)
    {
        _next = next;
    }


    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);

        /*if (context.Request.Path.Value.Contains(".js"))
        {
            context.Response.Headers.Add("Content-Type", "application/javascript");
        }*/
    }
}