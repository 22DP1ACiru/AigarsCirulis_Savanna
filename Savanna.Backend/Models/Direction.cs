﻿namespace Savanna.Backend.Models
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

        public static Direction GetRandomDirection()
        {
            Array values = Enum.GetValues(typeof(Direction));
            Random random = new Random();
            return (Direction)values.GetValue(random.Next(values.Length));
        }
    }
}