﻿namespace Savanna.Backend
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Savanna.Backend.Constants;
    using Savanna.Backend.Interfaces;
    using Savanna.Backend.Models;

    /// <summary>
    /// Represents the game grid.
    /// </summary>
    public class GameGrid : IGameGrid
    {
        private readonly List<IAnimal> _animals;
        private readonly Random _random = new Random();

        /// <summary>
        /// Gets the width of the game grid.
        /// </summary>
        public int Width => GameConstants.GridWidth;

        /// <summary>
        /// Gets the height of the game grid.
        /// </summary>
        public int Height => GameConstants.GridHeight;

        // <summary>
        /// Initializes a new instance of the <see cref="GameGrid"/> class.
        /// </summary>
        /// <param name="animals">The list of animals on the grid.</param>
        public GameGrid(List<IAnimal> animals)
        {
            _animals = animals;
        }

        /// <summary>
        /// Checks if a position is valid within the game grid.
        /// </summary>
        /// <param name="position">The position to check.</param>
        /// <returns>True if the position is valid; otherwise, false.</returns>
        public bool IsPositionValid(Position position)
        {
            return position.X >= 0 && position.X < Width &&
                   position.Y >= 0 && position.Y < Height;
        }

        /// <summary>
        /// Checks if a position is occupied by an animal.
        /// </summary>
        /// <param name="position">The position to check.</param>
        /// <returns>True if the position is occupied; otherwise, false.</returns>
        public bool IsPositionOccupied(Position position)
        {
            return _animals.Any(a => a.IsAlive && a.Position.X == position.X && a.Position.Y == position.Y);
        }

        /// <summary>
        /// Gets a random empty position on the game grid.
        /// </summary>
        /// <returns>A random empty position.</returns>
        public Position GetRandomEmptyPosition()
        {
            Position position;

            do
            {
                position = new Position(
                    _random.Next(0, Width),
                    _random.Next(0, Height)
                );
            } while (IsPositionOccupied(position));

            return position;
        }

        /// <summary>
        /// Gets the animal at the specified position, if any.
        /// </summary>
        /// <param name="position">The position to check.</param>
        /// <returns>The animal at the position, or null if no animal is present.</returns>
        public IAnimal GetAnimalAtPosition(Position position)
        {
            return _animals.FirstOrDefault(a => a.IsAlive && a.Position.X == position.X && a.Position.Y == position.Y);
        }

    }
}