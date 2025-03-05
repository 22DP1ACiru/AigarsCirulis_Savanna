using Savanna.Backend.Interfaces;
using Savanna.Backend;

namespace Savanna.Tests
{
    [TestClass]
    public class GameGridTests
    {
        [TestMethod]
        public void GetRandomEmptyPosition_ShouldReturnValidPosition_WhenGridIsNotFull()
        {
            // Arrange
            var animals = new List<IAnimal>();
            var gameGrid = new GameGrid(animals);

            // Act
            var position = gameGrid.GetRandomEmptyPosition();

            // Assert
            Assert.IsTrue(gameGrid.IsPositionValid(position));
            Assert.IsFalse(gameGrid.IsPositionOccupied(position));
        }
    }
}
