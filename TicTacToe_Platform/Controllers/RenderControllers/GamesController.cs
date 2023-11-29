using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicTacToe_Platform.Helpers;
using TicTacToe_Platform.Models.Games;

namespace TicTacToe_Platform.Controllers.RenderControllers;

[Authorize]
[Route("")]
public class GamesController : Controller
{
    private readonly GamesUtility _gamesUtility;
    private readonly IdentityUtility _identityUtility;
    private readonly ConcurrentDictionary<string, GameSession> _gameSessions;


    public GamesController(GamesUtility gamesUtility, IdentityUtility identityUtility, ConcurrentDictionary<string, GameSession> gameSessions)
    {
        _gamesUtility = gamesUtility;
        _gameSessions = gameSessions;
        _identityUtility = identityUtility;
    }

    [HttpGet("game")]
    public IActionResult JoinGame([FromQuery(Name = "id")] string gameId, [FromQuery(Name = "p")] string? gamePassword)
    {
        if (_gamesUtility.TryFindGameToJoin(gameId, gamePassword, out var game))
        {
            if (_gameSessions.TryGetValue(game.Id, out var gameSession))
            {
                if (gameSession.Game.UserGameInfos.Count >= 2)
                {
                    return BadRequest("Game session is full");
                }

                var user = _identityUtility.GetCurrentUser(HttpContext);
                
                ViewBag.GameId = gameId;
                ViewBag.UserId = user.Id;
                return View("/Views/Pages/GamePage.cshtml");
            }

            return BadRequest("Can't find game session");
        }

        if (game is null)
        {
            return BadRequest("Incorrect game access data");
        }

        switch (game.GameStatus)
        {
            case GameStatus.Started:
                return BadRequest("Game already started");
            case GameStatus.Draw or GameStatus.PlayerDisconnected or GameStatus.PlayerWon:
                return BadRequest("Game session is ended");
        }
        
        return BadRequest("Game is unavailable");
    }
}