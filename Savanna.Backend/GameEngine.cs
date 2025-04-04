namespace Savanna.Backend
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Savanna.Backend.Constants;
    using Savanna.Backend.Exceptions;
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

        public bool IsStalled { get; private set; } = false;

        public int IterationCount { get; private set; }
        public int LivingAnimalCount
        {
            get
            {
                lock (_lock)
                {
                    return _animals.Count(a => a.IsAlive);
                }
            }
        }

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
                IsStalled = false;

                GameEngineMediator.Instance.RegisterGameEngine(this);
                GameGridMediator.Instance.RegisterGameGrid(_gameGrid);
            }
        }

        /// <summary>
        /// Updates the game state by letting each animal perform its action and removing dead animals.
        /// </summary>
        public void Update()
        {
            if (IsStalled) return;

            // Create a copy of the animal list to avoid modification issues during iteration
            List<IAnimal> animalsToAct;
            List<IAnimal> snapshotForAct;

            lock (_lock)
            {
                IterationCount++;

                foreach(var animal in _animals)
                {
                    animal.IncrementAge();
                }

                animalsToAct = _animals.Where(a => a.IsAlive).ToList();
                snapshotForAct = _animals.ToList();

                try
                {
                    foreach (var animal in animalsToAct)
                    {
                        if (animal.IsAlive)
                        {
                            animal.Act(snapshotForAct);
                        }
                    }

                    GameEngineMediator.Instance.ProcessPendingAnimals(); // Process births etc.

                    _animals.RemoveAll(a => !a.IsAlive); // Remove dead animals
                }
                catch (GridFullException gfex)
                {
                    IsStalled = true;
                    Console.WriteLine(string.Format(LoggingConstants.GameEngineUpdateStalled, gfex.Message));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(string.Format(LoggingConstants.GameEngineUnexpectedError, ex.Message));
                }
            }
        }

        /// <summary>
        /// Gets animal at specified position.
        /// </summary>
        /// <param name="position">Specified position.</param>
        /// <returns>Animal at position.</returns>
        public IAnimal GetAnimalAtPosition(Position position)
        {
            lock (_lock)
            {
                return _animals.FirstOrDefault(a => a.IsAlive && a.Position.Equals(position));
            }
        }

        public IAnimal GetAnimalById(Guid id)
        {
            lock (_lock)
            {
                // Find the first (and should be only) animal with this ID
                return _animals.FirstOrDefault(a => a.UniqueId == id);
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
                catch (GridFullException gfex)
                {
                    Console.WriteLine(string.Format(LoggingConstants.GameEngineAddAnimalFailed, animal.GetType().Name, gfex.Message));
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
                    Animals = _animals.Select(a => a.GetState()).ToList(),
                    LivingAnimalCount = _animals.Count(a => a.IsAlive)
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