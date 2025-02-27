namespace Savanna.Backend.Models
{
    public class Position
    {
        /// <summary>
        /// Gets or sets the X-coordinate (horizontal position).
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Gets or sets the Y-coordinate (vertical position).
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// Initializes a new instance of the Position class with the specified coordinates.
        /// </summary>
        /// <param name="x">The X-coordinate.</param>
        /// <param name="y">The Y-coordinate.</param>
        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Calculates the Euclidean distance between this position and another position.
        /// </summary>
        /// <param name="other">The other position.</param>
        /// <returns>The distance between the two positions.</returns>
        public double DistanceTo(Position other)
        {
            int dx = X - other.X;
            int dy = Y - other.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// Determines whether this position is equal to another position.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is not Position other)
                return false;

            return X == other.X && Y == other.Y;
        }
    }
}