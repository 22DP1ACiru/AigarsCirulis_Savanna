namespace Savanna.Backend.Configuration
{
    using System.Collections.Generic;

    public class AnimalConfiguration
    {
        public int DefaultVisionRange { get; set; }
        public double HealthDrainPerMove { get; set; }
        public int ReproductionProximityCounter { get; set; }
        public double ReproductionRange { get; set; }
        public Dictionary<string, AnimalTypeConfig> Animals { get; set; } = new Dictionary<string, AnimalTypeConfig>();
    }

    public class AnimalTypeConfig
    {
        public int VisionRange { get; set; }
        public int MovementSpeed { get; set; }
        public char Symbol { get; set; }
        public double MaxHealth { get; set; }
        public int PowerLevel { get; set; }

        public int? DigestionTime { get; set; }
        public double? GrazingThresholdPercentage { get; set; }
    }
}