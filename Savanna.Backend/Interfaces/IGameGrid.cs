namespace Savanna.Backend.Interfaces
{
    using Savanna.Backend.Models;

    public interface IGameGrid
    {
        /// <summary>
        /// Gets the width of the game grid.
        /// </summary>
        int Width { get; }

        /// <summary>
        /// Gets the height of the game grid.
        /// </summary>
        int Height { get; }

        /// <summary>
        /// Checks if a position is valid within the game grid.
        /// </summary>
        /// <param name="position">The position to check.</param>
        /// <returns>True if the position is valid; otherwise, false.</returns>
        bool IsPositionValid(Position position);

        // <summary>
        /// Checks if a position is occupied by an animal.
        /// </summary>
        /// <param name="position">The position to check.</param>
        /// <returns>True if the position is occupied; otherwise, false.</returns>
        bool IsPositionOccupied(Position position);

        // <summary>
        /// Gets a random empty position on the game grid.
        /// </summary>
        /// <returns>A random empty position.</returns>
        Position GetRandomEmptyPosition();

        /// <summary>
        /// Gets the animal at the specified position, if any.
        /// </summary>
        /// <param name="position">The position to check.</param>
        /// <returns>The animal at the position, or null if no animal is present.</returns>
        IAnimal GetAnimalAtPosition(Position position);
    }
}