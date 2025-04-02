namespace Savanna.Web.DTOs
{
    public class GridCellDto
    {
        public char Symbol { get; set; }
        public string Icon { get; set; }
        public Guid? AnimalId { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }
}
