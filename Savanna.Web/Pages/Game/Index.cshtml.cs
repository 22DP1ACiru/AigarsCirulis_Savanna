using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Savanna.Backend;
using Savanna.Backend.Animals;
using Savanna.Backend.Models;

[Authorize]
public class GameIndexModel : PageModel
{
    private readonly GameEngine _gameEngine;

    public string SessionId { get; private set; }

    public char[,] DisplayGrid { get; private set; }

    public GameIndexModel()
    {
        _gameEngine = new GameEngine();
        _gameEngine.Initialize();

        _gameEngine.AddAnimal(new Antelope(new Position(0, 0)));
        _gameEngine.AddAnimal(new Antelope(new Position(0, 0)));
        _gameEngine.AddAnimal(new Lion(new Position(0, 0)));
    }

    public void OnGet(string sessionId)
    {
        DisplayGrid = _gameEngine.GetDisplayGrid();
        SessionId = sessionId;
    }
}