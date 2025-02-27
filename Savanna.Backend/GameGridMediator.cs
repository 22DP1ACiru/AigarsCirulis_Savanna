namespace Savanna.Backend
{
    using System;
    using Savanna.Backend.Interfaces;
    using Savanna.Backend.Models;

    /// <summary>
    /// Mediator for accessing game grid functionality across the application.
    /// </summary>
    public class GameGridMediator
    {
        private static GameGridMediator _instance;
        private static readonly object _lockObject = new object();

        private IGameGrid _gameGrid;

        public static GameGridMediator Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lockObject)
                    {
                        if (_instance == null)
                        {
                            _instance = new GameGridMediator();
                        }
                    }
                }
                return _instance;
            }
        }

        private GameGridMediator() { }

        public void RegisterGameGrid(IGameGrid gameGrid)
        {
            _gameGrid = gameGrid;
        }

        /// <summary>
        /// Checks if a position is valid within the game grid.
        /// </summary>
        /// <param name="position">The position to check.</param>
        /// <returns>True if the position is valid; otherwise, false.</returns>
        public bool IsPositionValid(Position position)
        {
            return _gameGrid != null && _gameGrid.IsPositionValid(position);
        }

        /// <summary>
        /// Checks if a position is occupied by an animal.
        /// </summary>
        /// <param name="position">The position to check.</param>
        /// <returns>True if the position is occupied; otherwise, false.</returns>
        public bool IsPositionOccupied(Position position)
        {
            return _gameGrid != null && _gameGrid.IsPositionOccupied(position);
        }

        /// <summary>
        /// Finds an empty position adjacent to the given position.
        /// </summary>
        /// <param name="position">The reference position.</param>
        /// <returns>Adjacent empty position, or null if none available.</returns>
        public Position FindEmptyAdjacentPosition(Position position)
        {
            if (_gameGrid == null) return null;

            // Check all 8 directions for an empty adjacent position
            foreach (Direction direction in Enum.GetValues(typeof(Direction)))
            {
                Position offset = direction.GetOffset();
                Position adjacentPos = new Position(
                    position.X + offset.X,
                    position.Y + offset.Y
                );

                if (IsPositionValid(adjacentPos) && !IsPositionOccupied(adjacentPos))
                {
                    return adjacentPos;
                }
            }

            return null;
        }
    }
}