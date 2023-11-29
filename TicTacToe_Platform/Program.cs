using MySql.Data.MySqlClient;
using TicTacToe_Platform.Helpers;
using System.Collections.Concurrent;
using TicTacToe_Platform.Middlewares;
using TicTacToe_Platform.Models.Games;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.WebSockets;
using TicTacToe_Platform.Controllers.Hubs;
using TicTacToe_Platform.Models.Configurations;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

var config = builder.Configuration;

var appConfiguration = config.GetSection("AppConfiguration").Get<AppConfiguration>();
var connectionString = config.GetConnectionString("MySql");
var gameSessionConnectionString = config.GetConnectionString("GameSessionSql");
var signUpConfig = config.GetSection("SignUpConfiguration").Get<SignUpConfiguration>();
var authConfig = config.GetSection("AuthorizationConfiguration").Get<AuthorizationConfiguration>();
AuthorizationConfiguration.TokenLiveTime = TimeSpan.FromSeconds(authConfig.TokenLiveTimeSeconds);

var gameSessions = new ConcurrentDictionary<string, GameSession>();
var cryptoUtility = new CryptoUtility(authConfig, signUpConfig);
var identityUtility = new IdentityUtility(cryptoUtility);
var sqlConnection = new MySqlConnection(connectionString);
var gameSessionConnection = new MySqlConnection(gameSessionConnectionString);
var userUtility = new UserUtility(sqlConnection, cryptoUtility);
var gameUtility = new GamesUtility(sqlConnection, cryptoUtility, gameSessionConnection);
var dataValidator = new DataValidator(cryptoUtility, userUtility);
/*var gameSessionHandler = new GameSessionHandler(gameSessions, gameUtility);*/

/*services.AddGrpc();*/
services.AddWebSockets((x =>
{
    x.KeepAliveInterval = TimeSpan.FromSeconds(15);
}));
var razorBuilder = services.AddRazorPages()
    .AddRazorPagesOptions(options => { options.RootDirectory = "/Pages"; });

services.AddControllers();
services.AddControllersWithViews();
services.AddSignalR();

services.AddSingleton(userUtility);
services.AddSingleton(gameUtility);
services.AddSingleton(gameSessions);
services.AddSingleton(sqlConnection);
services.AddSingleton(dataValidator);
services.AddSingleton(cryptoUtility);
services.AddSingleton(identityUtility);
/*services.AddSingleton(gameSessionHandler);*/

services.AddAuthentication("Authentication")
    .AddScheme<AuthenticationSchemeOptions, AuthenticationHandler>("Authentication", null);


if (appConfiguration.IsDev)
{
    razorBuilder.AddRazorRuntimeCompilation();
}

var app = builder.Build();

app.UseMiddleware<SqlConnectionMiddleware>();
app.UseMiddleware<UserFilterMiddleware>();

/*app.MapGrpcService<TicTacToe_Platform.Controllers.Grpc.GameSessionManager>();*/
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.UseStaticFiles();
app.UseRouting();
app.UseExceptionHandler("/Exception/Handler");
app.UseAuthentication();
app.UseAuthorization();
app.UseEndpoints(x =>
{
    x.MapHub<GameSessionManagerHub>("/gameSessionHub");
});
/*app.MapGrpcService<GameSessionManager>();*/
app.Run();