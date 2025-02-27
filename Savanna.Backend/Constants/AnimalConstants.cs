namespace Savanna.Backend.Constants
{
    using Savanna.Backend.Configuration;

    public static class AnimalConstants
    {
        private static readonly ConfigurationService _configService = ConfigurationService.Instance;

        // Shared constants
        public static int DefaultVisionRange => _configService.AnimalConfig.DefaultVisionRange;
        public static double HealthDrainPerMove => 0.5;

        // Antelope constants
        public static int AntelopeVisionRange => _configService.GetAnimalConfig("Antelope").VisionRange;
        public static int AntelopeMovementSpeed => _configService.GetAnimalConfig("Antelope").MovementSpeed;
        public static char AntelopeSymbol => _configService.GetAnimalConfig("Antelope").Symbol;
        public static double AntelopeMaxHealth => _configService.GetAnimalConfig("Antelope").MaxHealth ?? 10.0;

        // Lion constants
        public static int LionVisionRange => _configService.GetAnimalConfig("Lion").VisionRange;
        public static int LionMovementSpeed => _configService.GetAnimalConfig("Lion").MovementSpeed;
        public static char LionSymbol => _configService.GetAnimalConfig("Lion").Symbol;
        public static int LionDigestionTime => _configService.GetAnimalConfig("Lion").DigestionTime ?? 2;
        public static double LionMaxHealth => _configService.GetAnimalConfig("Lion").MaxHealth ?? 15.0;
    }
}