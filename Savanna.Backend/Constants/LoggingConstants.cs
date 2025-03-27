namespace Savanna.Backend.Constants
{
    public static class LoggingConstants
    {
        public const string AnimalFactoryPrefix = "[AnimalFactory]";
        public const string AnimalFactoryWarningInvalidState = AnimalFactoryPrefix + " Warning: CreateAnimalFromState called with null or invalid state.";
        public const string AnimalFactoryWarningTypeNotFound = AnimalFactoryPrefix + " Warning: No built-in type or registered plugin found for animal type '{0}'.";
        public const string AnimalFactoryErrorCreatingState = AnimalFactoryPrefix + " Error during creation or state loading for animal type '{0}'. Error: {1}";
    }
}