namespace Savanna.Backend.Constants
{
    public static class LoggingConstants
    {
        public const string AnimalFactoryPrefix = "[AnimalFactory]";
        public const string AnimalFactoryWarningInvalidState = AnimalFactoryPrefix + " Warning: CreateAnimalFromState called with null or invalid state.";
        public const string AnimalFactoryWarningTypeNotFound = AnimalFactoryPrefix + " Warning: No built-in type or registered plugin found for animal type '{0}'.";
        public const string AnimalFactoryErrorCreatingState = AnimalFactoryPrefix + " Error during creation or state loading for animal type '{0}'. Error: {1}";

        public const string GameEnginePrefix = "[GameEngine]";
        public const string GameEngineGridFullAct = GameEnginePrefix + "[ERROR][Session Update] Grid full exception during animal action: {0}";
        public const string GameEngineExceptionAct = GameEnginePrefix + "[ERROR][Session Update] Exception during Act for {0}: {1}";
        public const string GameEngineGridFullPending = GameEnginePrefix + "[ERROR][Session Update] Grid full exception during ProcessPendingAnimals: {0}";
        public const string GameEngineExceptionPending = GameEnginePrefix + "[ERROR][Session Update] Exception during ProcessPendingAnimals: {0}";
        public const string GameEngineUnexpectedError = GameEnginePrefix + "[ERROR][Session Update] Unexpected error during update loop: {0}";
        public const string GameEngineAddAnimalFailed = GameEnginePrefix + "[WARNING] Failed to add animal {0}: {1}";
        public const string GameEngineUpdateStalled = GameEnginePrefix + "[NOTIFICATION][Session Update] Game Stalled: {0}";
    }
}