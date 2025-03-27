namespace Savanna.Backend.Models.State
{
    public class AnimalStateDto
    {
        // Common properties from AnimalBase/IAnimal
        public string AnimalType { get; set; } // e.g., "Lion", "Antelope", plugin type name
        public Position Position { get; set; }
        public double Health { get; set; }
        public bool IsAlive { get; set; }

        // Specific properties (can be nullable if not applicable to all)
        public int? DigestionTimeRemaining { get; set; } // From ICarnivore (Lion)
    }
}
