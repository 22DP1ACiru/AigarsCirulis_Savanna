namespace Savanna.Backend.Interfaces
{
    public interface IGrazingStrategy
    {
        bool ShouldGraze(IHerbivore herbivore, IAnimal herbivoreAsAnimal, List<IAnimal> visibleAnimals);
    }
}