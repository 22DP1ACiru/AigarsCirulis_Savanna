using Savanna.Backend;
using Savanna.Backend.Animals;
using Savanna.Backend.Interfaces;
using Savanna.Backend.Models;

namespace Savanna.Tests
{
    [TestClass]
    public class AnimalBaseTests
    {
        [TestInitialize]
        public void Setup()
        {
            // Create and register a mock game grid
            var mockGrid = new GameGrid(new List<IAnimal>());
            GameGridMediator.Instance.RegisterGameGrid(mockGrid);
        }

        [TestMethod]
        public void Move_ShouldUpdatePosition_WhenDirectionIsValid()
        {
            // Arrange
            var position = new Position(5, 5);
            var antelope = new Antelope(position);
            var initialPosition = new Position(antelope.Position.X, antelope.Position.Y);

            // Act
            antelope.Move(Direction.Right);

            // Assert
            Assert.AreEqual(initialPosition.X + 1, antelope.Position.X);
            Assert.AreEqual(initialPosition.Y, antelope.Position.Y);
        }

        [TestMethod]
        public void Move_ShouldNotUpdatePosition_WhenDirectionIsInvalid()
        {
            // Arrange
            var position = new Position(0, 0);
            var antelope = new Antelope(position);
            var initialPosition = new Position(antelope.Position.X, antelope.Position.Y);

            // Act
            antelope.Move(Direction.Left); // Trying to move left from (0, 0)

            // Assert
            Assert.AreEqual(initialPosition.X, antelope.Position.X);
            Assert.AreEqual(initialPosition.Y, antelope.Position.Y);
        }

        [TestMethod]
        public void Kill_ShouldSetIsAliveToFalse()
        {
            // Arrange
            var position = new Position(5, 5);
            var antelope = new Antelope(position);

            // Act
            antelope.Kill();

            // Assert
            Assert.IsFalse(antelope.IsAlive);
        }
    }
}