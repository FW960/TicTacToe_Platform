using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicTacToe_Platform.Helpers;
using TicTacToe_Platform.Models.Games;
using TicTacToe_Platform.Views.Pages;

namespace TicTacToe_Platform.Controllers.RenderControllers;

[Authorize]
public class AccountGamesController : Controller
{
    private readonly GamesUtility _gamesUtility;
    private readonly IdentityUtility _identityUtility;
    private readonly ConcurrentDictionary<string, GameSession> _gameSessions;

    public AccountGamesController(GamesUtility gamesUtility, IdentityUtility identityUtility, ConcurrentDictionary<string, GameSession> gameSessions)
    {
        _gamesUtility = gamesUtility;
        _identityUtility = identityUtility;
        _gameSessions = gameSessions;
    }
    
    [HttpGet("Games")]
    [HttpGet("Account")]
    public IActionResult Games()
    {
        var model = new AccountGamesModel
        {
            IsGames = HttpContext.Request.Path.Value.Contains("games", StringComparison.OrdinalIgnoreCase),
            UserPlayedGames = _gamesUtility.GetUserPlayedGames(_identityUtility.GetCurrentUser(HttpContext)!.Id),
            AvailableGames = _gameSessions,
            User = _identityUtility.GetCurrentUser(HttpContext)
        };
        
        return View("/Views/Pages/AccountGamesPage.cshtml", model);
    }  
    
}