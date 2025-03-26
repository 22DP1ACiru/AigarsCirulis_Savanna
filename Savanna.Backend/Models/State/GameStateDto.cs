namespace Savanna.Backend.Models.State
{
    public class GameStateDto
    {
        public int IterationCount { get; set; }
        public List<AnimalStateDto> Animals { get; set; } = new List<AnimalStateDto>();
    }
}