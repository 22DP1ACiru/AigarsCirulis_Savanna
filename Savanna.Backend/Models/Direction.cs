namespace Savanna.Backend.Models
{
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right,
        UpLeft,
        UpRight,
        DownLeft,
        DownRight
    }

    public static class DirectionExtensions
    {
        private static readonly Random _random = new Random();

        /// <summary>
        /// Gets the offset for the specified direction.
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static Position GetOffset(this Direction direction)
        {
            return direction switch
            {
                Direction.Up => new Position(0, -1),
                Direction.Down => new Position(0, 1),
                Direction.Left => new Position(-1, 0),
                Direction.Right => new Position(1, 0),
                Direction.UpLeft => new Position(-1, -1),
                Direction.UpRight => new Position(1, -1),
                Direction.DownLeft => new Position(-1, 1),
                Direction.DownRight => new Position(1, 1),
                _ => new Position(0, 0)
            };
        }

        /// <summary>
        /// Gets a random direction.
        /// </summary>
        /// <returns></returns>
        public static Direction GetRandomDirection()
        {
            Array values = Enum.GetValues(typeof(Direction));
            return (Direction)values.GetValue(_random.Next(values.Length));
        }

        /// <summary>
        /// Gets a list of directions that lead to a valid position on the grid from the current position.
        /// Note: Only checks for grid boundaries.
        /// </summary>
        /// <param name="currentPosition">The starting position.</param>
        /// <returns>A list of valid directions.</returns>
        public static List<Direction> GetValidMoveDirections(Position currentPosition)
        {
            var validDirections = new List<Direction>();
            var allDirections = (Direction[])Enum.GetValues(typeof(Direction));

            foreach (Direction direction in allDirections)
            {
                Position offset = direction.GetOffset();
                Position potentialNextPos = new Position(currentPosition.X + offset.X, currentPosition.Y + offset.Y);

                // Check ONLY if the position is within grid bounds using the Mediator
                if (GameGridMediator.Instance.IsPositionValid(potentialNextPos))
                {
                    validDirections.Add(direction);
                }
            }
            return validDirections;
        }
    }
}