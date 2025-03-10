namespace Savanna.Backend.Interfaces
{
    public interface ICarnivore
    {
        int DigestionTime { get; }
        int DigestionTimeRemaining { get; set; }

        void Hunt(IAnimal prey);
        void RestoreHealth();
    }
}