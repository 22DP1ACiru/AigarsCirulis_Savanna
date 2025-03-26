using Savanna.Backend.Animals;
using Savanna.Backend.Models;
using Savanna.Backend;
using Savanna.Backend.Constants;
using Savanna.Backend.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Savanna.Tests
{
    [TestClass]
    public class GameEngineTests
    {
        private Mock<IAnimalFactory> _mockAnimalFactory;

        [TestInitialize]
        public void Setup()
        {
            _mockAnimalFactory = new Mock<IAnimalFactory>();
        }

        [TestMethod]
        public void AddAnimal_ShouldAddAnimalToGameGrid()
        {
            // Arrange
            var gameEngine = new GameEngine(_mockAnimalFactory.Object);
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
            var gameEngine = new GameEngine(_mockAnimalFactory.Object);
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
