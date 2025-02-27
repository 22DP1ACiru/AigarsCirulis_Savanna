using Savanna.Backend.Animals;
using Savanna.Backend.Models;
using Savanna.Backend;
using Savanna.Backend.Constants;

namespace Savanna.Tests
{
    [TestClass]
    public class GameEngineTests
    {
        [TestMethod]
        public void AddAnimal_ShouldAddAnimalToGameGrid()
        {
            // Arrange
            var gameEngine = new GameEngine();
            gameEngine.Initialize();
            var antelope = new Antelope(new Position(0, 0));

            // Act
            gameEngine.AddAnimal(antelope);

            // Assert
            var displayGrid = gameEngine.GetDisplayGrid();
            Assert.AreEqual(antelope.Symbol, displayGrid[antelope.Position.X, antelope.Position.Y]);
        }

        [TestMethod]
        public void Update_ShouldRemoveDeadAnimals()
        {
            // Arrange
            var gameEngine = new GameEngine();
            gameEngine.Initialize();
            var antelope = new Antelope(new Position(0, 0));
            gameEngine.AddAnimal(antelope);
            antelope.Kill();

            // Act
            gameEngine.Update();

            // Assert
            var displayGrid = gameEngine.GetDisplayGrid();
            Assert.AreEqual(GameConstants.EmptyCellSymbol, displayGrid[0, 0]);
        }
    }
}
