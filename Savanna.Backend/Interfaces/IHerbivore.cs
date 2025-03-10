namespace Savanna.Backend.Interfaces
{
    public interface IHerbivore
    {
        double GrazingThresholdPercentage { get; }

        void Graze();
    }
}