using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicTacToe_Platform.Helpers;
using TicTacToe_Platform.Views.Pages;

namespace TicTacToe_Platform.Controllers.RenderControllers;

[Authorize]
public class AccountGamesController : Controller
{
    private readonly GamesUtility _gamesUtility;
    private readonly IdentityUtility _identityUtility;

    public AccountGamesController(GamesUtility gamesUtility, IdentityUtility identityUtility)
    {
        _gamesUtility = gamesUtility;
        _identityUtility = identityUtility;
    }
    
    [HttpGet("Games")]
    [HttpGet("Account")]
    public IActionResult Games()
    {
        var model = new AccountGamesModel
        {
            IsGames = HttpContext.Request.Path.Value.Contains("games", StringComparison.OrdinalIgnoreCase),
            UserPlayedGames = _gamesUtility.GetUserPlayedGames(_identityUtility.GetCurrentUser(HttpContext)!.Id),
            AvailableGames = _gamesUtility.GetAvailableGames(),
            User = _identityUtility.GetCurrentUser(HttpContext)
        };
        
        return View("/Views/Pages/AccountGamesPage.cshtml", model);
    }  
    
}