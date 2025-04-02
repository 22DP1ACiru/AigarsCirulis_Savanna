using Savanna.Core.Models;
using Savanna.Web.DTOs;
using Savanna.Backend.Configuration;
using Savanna.Backend.Constants;
using Savanna.Backend.Interfaces;
using Savanna.Backend.Models;

namespace Savanna.Web.Utils
{
    public static class PayloadUtils
    {
        public static object CreateWebPayload(GameSession session, ILogger logger = null)
        {
            if (session == null) return null;

            // Get raw data from engine
            char[,] grid = session.Engine.GetDisplayGrid();
            int iterationCount = session.Engine.IterationCount;
            int livingAnimalCount = session.Engine.LivingAnimalCount;
            var configService = ConfigurationService.Instance;

            // Transform
            var cells = new List<GridCellDto>();
            int width = grid.GetLength(0);
            int height = grid.GetLength(1);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    char symbol = grid[x, y];
                    Guid? animalId = null;
                    string icon = null;
                    if (symbol != GameConstants.EmptyCellSymbol)
                    {
                        IAnimal animal = session.Engine.GetAnimalAtPosition(new Position(x, y));
                        if (animal != null)
                        {
                            animalId = animal.UniqueId;
                            var config = configService.GetAnimalConfig(animal.AnimalTypeName);
                            icon = config?.Icon;
                        }
                        else
                        {
                            logger?.LogWarning("Symbol '{Symbol}' found at ({X},{Y}) but GetAnimalAtPosition returned null in CreateWebPayload.", symbol, x, y);
                        }
                    }
                    cells.Add(new GridCellDto { Symbol = symbol, Icon = icon, AnimalId = animalId, X = x, Y = y });
                }
            }
            return new { Cells = cells, IterationCount = iterationCount, LivingAnimalCount = livingAnimalCount };
        }
    }
}