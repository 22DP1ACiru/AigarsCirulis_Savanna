namespace Savanna.Backend.Interfaces
{
    public interface IGameEngine
    {
        int IterationCount { get; }

        /// <summary>
        /// Initializes the game engine by creating a new game grid and clearing existing animals.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Updates the game state by letting each animal perform its action and removing dead animals.
        /// </summary>
        void Update();

        /// <summary>
        /// Adds a new animal of the specified type at a random empty position on the game grid.
        /// </summary>
        /// <param name="animal">Animal object to add.</param>
        void AddAnimal(IAnimal animal);

        /// <summary>
        /// Generates a display grid representing the current state of the game.
        /// </summary>
        /// <returns>A 2D character array representing the game grid with animals.</returns>
        char[,] GetDisplayGrid();
    }
}