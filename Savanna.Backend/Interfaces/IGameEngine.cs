namespace Savanna.Backend.Interfaces
{
    public interface IGameEngine
    {
        void Initialize();
        void Update();
        void AddAnimal(char animalType);
        char[,] GetDisplayGrid();
    }
}