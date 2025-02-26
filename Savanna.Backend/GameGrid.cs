namespace Savanna.Backend
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Savanna.Backend.Constants;
    using Savanna.Backend.Interfaces;
    using Savanna.Backend.Models;

    public class GameGrid : IGameGrid
    {
        private readonly List<IAnimal> _animals;
        private readonly Random _random = new Random();

        public int Width => GameConstants.GridWidth;
        public int Height => GameConstants.GridHeight;

        public GameGrid(List<IAnimal> animals)
        {
            _animals = animals;
        }

        public bool IsPositionValid(Position position)
        {
            return position.X >= 0 && position.X < Width &&
                   position.Y >= 0 && position.Y < Height;
        }

        public bool IsPositionOccupied(Position position)
        {
            return _animals.Any(a => a.IsAlive && a.Position.X == position.X && a.Position.Y == position.Y);
        }

        public IAnimal GetAnimalAt(Position position)
        {
            return _animals.FirstOrDefault(a => a.IsAlive && a.Position.X == position.X && a.Position.Y == position.Y);
        }

        public Position GetRandomEmptyPosition()
        {
            Position position;

            do
            {
                position = new Position(
                    _random.Next(0, Width),
                    _random.Next(0, Height)
                );
            } while (IsPositionOccupied(position));

            return position;
        }
    }
}