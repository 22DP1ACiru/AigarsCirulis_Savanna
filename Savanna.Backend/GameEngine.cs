namespace Savanna.Backend
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Savanna.Backend.Constants;
    using Savanna.Backend.Interfaces;
    using Savanna.Backend.Models;
    using Savanna.Backend.Models.State;

    /// <summary>
    /// Core engine that manages animals, updates the game state, and generates the display grid.
    /// </summary>
    public class GameEngine : IGameEngine
    {
        private readonly List<IAnimal> _animals = new List<IAnimal>();
        private readonly object _lock = new object();
        private IGameGrid _gameGrid;
        private readonly IAnimalFactory _animalFactory;

        public int IterationCount { get; private set; }

        public GameEngine(IAnimalFactory animalFactory) 
        {
            _animalFactory = animalFactory ?? throw new ArgumentNullException(nameof(animalFactory));
        }

        /// <summary>
        /// Initializes the game engine by creating a new game grid and clearing existing animals.
        /// </summary>
        public void Initialize()
        {
            lock (_lock) // Ensure thread safety
            {
                _animals.Clear();
                IterationCount = 0;
                _gameGrid = new GameGrid(_animals);

                GameEngineMediator.Instance.RegisterGameEngine(this);
                GameGridMediator.Instance.RegisterGameGrid(_gameGrid);
            }
        }

        /// <summary>
        /// Updates the game state by letting each animal perform its action and removing dead animals.
        /// </summary>
        public void Update()
        {
            // Create a copy of the animal list to avoid modification issues during iteration
            List<IAnimal> animalsToAct;
            List<IAnimal> snapshotForAct;

            lock (_lock)
            {
                IterationCount++;

                animalsToAct = _animals.Where(a => a.IsAlive).ToList();
                snapshotForAct = _animals.ToList();

                foreach (var animal in animalsToAct)
                {
                    if (animal.IsAlive)
                    {
                        try
                        {
                            animal.Act(snapshotForAct);
                        }
                        catch (InvalidOperationException ioex) when (ioex.Message.Contains("Grid is full"))
                        {
                            Console.WriteLine($"[ERROR][Session Update] Grid full exception during animal action: {ioex.Message}");
                            break; // Stop processing actions for this tick if grid is full during Act
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[ERROR][Session Update] Exception during Act for {animal.GetType().Name}: {ex.Message}");
                        }
                    }
                }

                try
                {
                    GameEngineMediator.Instance.ProcessPendingAnimals(); // Process births etc.
                }
                catch (InvalidOperationException ioex) when (ioex.Message.Contains("Grid is full"))
                {
                    Console.WriteLine($"[ERROR][Session Update] Grid full exception during ProcessPendingAnimals: {ioex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR][Session Update] Exception during ProcessPendingAnimals: {ex.Message}");
                }

                // Remove dead animals
                _animals.RemoveAll(a => !a.IsAlive);

            }
        }

        /// <summary>
        /// Adds a new animal at a random empty position on the game grid.
        /// </summary>
        /// <param name="animal">The animal to add.</param>
        public void AddAnimal(IAnimal animal)
        {
            if (animal == null) return;

            lock (_lock)
            {
                try
                {
                    Position position = _gameGrid.GetRandomEmptyPosition();
                    animal.Position = position;
                    _animals.Add(animal);
                }
                catch (InvalidOperationException ioex) when (ioex.Message.Contains("Grid is full") || ioex.Message.Contains("find empty position"))
                {
                    Console.WriteLine($"[WARNING] Failed to add animal {animal.GetType().Name}: {ioex.Message}");
                }
            }
        }

        /// <summary>
        /// Generates a display grid representing the current state of the game.
        /// </summary>
        /// <returns>A 2D character array representing the game grid with animals.</returns>
        public char[,] GetDisplayGrid()
        {
            lock (_lock)
            {
                char[,] displayGrid = new char[GameConstants.GridWidth, GameConstants.GridHeight];

                // Initialize with empty spaces
                for (int x = 0; x < GameConstants.GridWidth; x++)
                {
                    for (int y = 0; y < GameConstants.GridHeight; y++)
                    {
                        displayGrid[x, y] = GameConstants.EmptyCellSymbol;
                    }
                }

                // Add animals to the display grid
                foreach (var animal in _animals.Where(a => a.IsAlive))
                {
                    int x = Math.Clamp(animal.Position.X, 0, GameConstants.GridWidth - 1);
                    int y = Math.Clamp(animal.Position.Y, 0, GameConstants.GridHeight - 1);
                    displayGrid[x, y] = animal.Symbol;
                }

                return displayGrid;
            }
        }

        public GameStateDto GetGameState()
        {
            lock (_lock)
            {
                var gameState = new GameStateDto
                {
                    IterationCount = this.IterationCount,
                    Animals = _animals.Select(a => a.GetState()).ToList()   
                };
                return gameState;
            }
        }

        public void LoadGameState(GameStateDto gameState)
        {
            if (gameState == null) throw new ArgumentNullException(nameof(gameState));

            lock (_lock)
            {
                _animals.Clear();
                this.IterationCount = gameState.IterationCount;

                foreach (var animalState in gameState.Animals)
                {
                    IAnimal animal = _animalFactory.CreateAnimalFromState(animalState);
                    if (animal != null)
                    {
                        _animals.Add(animal);
                    }
                }

                _gameGrid = new GameGrid(_animals); // Recreate grid
                GameGridMediator.Instance.RegisterGameGrid(_gameGrid);
            }
        }
    }
}