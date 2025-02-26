namespace Savanna.Backend.Interfaces
{
    using Savanna.Backend.Models;

    public interface IGameGrid
    {
        int Width { get; }
        int Height { get; }
        bool IsPositionValid(Position position);
        bool IsPositionOccupied(Position position);
        IAnimal GetAnimalAt(Position position);
        Position GetRandomEmptyPosition();
    }
}