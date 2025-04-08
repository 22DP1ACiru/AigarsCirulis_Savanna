namespace Savanna.Backend.Exceptions
{
    public class GridFullException : InvalidOperationException
    {
        public GridFullException() : base("The grid is full or no adjacent space is available.") { }
        public GridFullException(string message) : base(message) { }
        public GridFullException(string message, Exception inner) : base(message, inner) { }
    }
}