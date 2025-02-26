namespace Savanna.Backend
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Savanna.Backend.Animals;
    using Savanna.Backend.Constants;
    using Savanna.Backend.Interfaces;
    using Savanna.Backend.Models;

    /// <summary>
    /// Core engine that manages animals, updates the game state, and generates the display grid.
    /// </summary>
    public class GameEngine : IGameEngine
    {
        private readonly List<IAnimal> _animals = new List<IAnimal>();
        private IGameGrid _gameGrid;

        /// <summary>
        /// Initializes the game engine by creating a new game grid and clearing existing animals.
        /// </summary>
        public void Initialize()
        {
            _animals.Clear();
            _gameGrid = new GameGrid(_animals);
        }

        /// <summary>
        /// Updates the game state by letting each animal perform its action and removing dead animals.
        /// </summary>
        public void Update()
        {
            // Create a copy of the animal list to avoid modification issues during iteration
            var animalsCopy = _animals.Where(a => a.IsAlive).ToList();

            foreach (var animal in animalsCopy)
            {
                animal.Act(_animals);
            }

            // Remove dead animals
            _animals.RemoveAll(a => !a.IsAlive);
        }

        /// <summary>
        /// Adds a new animal of the specified type at a random empty position on the game grid.
        /// </summary>
        /// <param name="animalType">The character symbol representing the type of animal to add.</param>
        public void AddAnimal(char animalType)
        {
            Position position = _gameGrid.GetRandomEmptyPosition();

            switch (animalType)
            {
                case AnimalConstants.AntelopeSymbol:
                    _animals.Add(new Antelope(position));
                    break;

                case AnimalConstants.LionSymbol:
                    _animals.Add(new Lion(position));
                    break;
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
    }
}