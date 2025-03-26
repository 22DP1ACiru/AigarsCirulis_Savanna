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
            lock (_lock)
            {
                IterationCount++;
            }

            // Create a copy of the animal list to avoid modification issues during iteration
            List<IAnimal> animalsCopy;
            lock (_lock)
            {
                animalsCopy = _animals.Where(a => a.IsAlive).ToList();
            }

            List<IAnimal> snapshotForAll;
            lock (_lock)
            { 
                snapshotForAll = _animals.ToList(); 
            }

            foreach (var animal in animalsCopy)
            {
                // Ensure animal is still alive before acting (could have been killed by another animal in the same tick)
                if (animal.IsAlive)
                {
                    animal.Act(snapshotForAll); // Pass consistent snapshot
                }
            }
    

            lock (_lock)
            {
                // Remove dead animals
                _animals.RemoveAll(a => !a.IsAlive);

                // Process any new animals requested during the update
                GameEngineMediator.Instance.ProcessPendingAnimals();
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
                Position position = _gameGrid.GetRandomEmptyPosition();
                animal.Position = position; // Set the animal's position
                _animals.Add(animal);
            }
        }

        /// <summary>
        /// Generates a display grid representing the current state of the game.
        /// </summary>
        /// <returns>A 2D character array representing the game grid with animals.</returns>
        public char[,] GetDisplayGrid()
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

        public GameStateDto GetGameState()
        {
            lock (_lock) // Ensure thread safety while reading state
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