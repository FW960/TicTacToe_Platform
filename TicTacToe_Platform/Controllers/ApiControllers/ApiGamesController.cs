using Microsoft.AspNetCore.Mvc;
using TicTacToe_Platform.Helpers;
using System.Collections.Concurrent;
using TicTacToe_Platform.Models.Games;
using Microsoft.AspNetCore.Authorization;
using TicTacToe_Platform.Models.UserManagementModels;

namespace TicTacToe_Platform.Controllers.ApiControllers;

[Authorize]
[Route("Api/Games")]
public class ApiGamesController : Controller
{
    private readonly GamesUtility _gamesUtility;
    private readonly IdentityUtility _identityUtility;
    private readonly ConcurrentDictionary<string, GameSession> _gameSessions;

    public ApiGamesController(GamesUtility gamesUtility, ConcurrentDictionary<string, GameSession> gameSessions,
        IdentityUtility identityUtility)
    {
        _gamesUtility = gamesUtility;
        _gameSessions = gameSessions;
        _identityUtility = identityUtility;
    }

    [HttpPost("Create")]
    public IActionResult Create([FromBody] CreateGameModel model)
    {
        if (_gamesUtility.CreateGame(model, out var game))
        {
            HttpContext.Response.Headers.Add("Redirect",
                $"/game?id={game.Id}{(string.IsNullOrEmpty(model.GamePassword) ? "" : $"&p={model.GamePassword}")}");

            var currentUser = _identityUtility.GetCurrentUser(HttpContext);

            _gameSessions.TryAdd(game.Id, new GameSession
            {
                Game = game,
                CurrentUserIdTurn = currentUser.Id
            });
            return Ok();
        }

        return BadRequest("Error trying to create a game");
    }
}