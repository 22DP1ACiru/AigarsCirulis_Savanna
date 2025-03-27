using Savanna.Backend.Interfaces;
using Savanna.Backend.Models.State;
using Savanna.Backend.Animals;
using Savanna.Backend.Plugins;
using Savanna.Backend.Constants;

namespace Savanna.Backend.Factories
{
    public class AnimalFactory : IAnimalFactory
    {
        private readonly PluginManager _pluginManager = PluginManager.Instance;

        public IAnimal CreateAnimalFromState(AnimalStateDto state)
        {
            if (state == null || string.IsNullOrEmpty(state.AnimalType))
            {
                Console.WriteLine(LoggingConstants.AnimalFactoryWarningInvalidState);
                return null;
            }

            IAnimal animal = null;

            try
            {
                if (state.AnimalType == nameof(Antelope))
                {
                    animal = new Antelope(state.Position);
                }
                else if (state.AnimalType == nameof(Lion))
                {
                    animal = new Lion(state.Position);
                }
                else
                {
                    var plugin = _pluginManager.RegisteredPlugins.Values
                                        .FirstOrDefault(p => p.AnimalType == state.AnimalType);

                    if (plugin != null)
                    {
                        animal = plugin.CreateAnimal(state.Position);
                    }
                    else
                    {
                        Console.WriteLine(string.Format(LoggingConstants.AnimalFactoryWarningTypeNotFound, state.AnimalType));
                        return null;
                    }
                }

                if (animal != null)
                {
                    animal.LoadState(state);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format(LoggingConstants.AnimalFactoryErrorCreatingState, state.AnimalType, ex.Message));
                return null;
            }

            return animal;
        }
    }
}