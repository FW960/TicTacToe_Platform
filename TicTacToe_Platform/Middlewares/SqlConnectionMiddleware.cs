using MySql.Data.MySqlClient;

namespace TicTacToe_Platform.Middlewares;

public class SqlConnectionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly MySqlConnection _sqlConnection;
    
    public SqlConnectionMiddleware(MySqlConnection sqlConnection, RequestDelegate next)
    {
        _sqlConnection = sqlConnection;
        _next = next;
    }


    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            if (!context.Request.Path.Value.Contains("assets") && 
                !context.Request.Path.Value.Contains("gameSessionHub"))
                _sqlConnection.Open();

            await _next(context);
        }
        catch
        {
            //todo logger
        }
        finally
        {
            _sqlConnection.Close();
        }
    }
}