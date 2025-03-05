namespace Savanna.Backend
{
    using System;
    using System.Collections.Generic;
    using Savanna.Backend.Interfaces;

    /// <summary>
    /// Mediator pattern implementation to allow indirect communication between animals and the game engine.
    /// </summary>
    public class GameEngineMediator
    {
        private static GameEngineMediator _instance;
        private static readonly object _lockObject = new object();

        private IGameEngine _gameEngine;
        private List<IAnimal> _pendingAnimals = new List<IAnimal>();

        public static GameEngineMediator Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lockObject)
                    {
                        if (_instance == null)
                        {
                            _instance = new GameEngineMediator();
                        }
                    }
                }
                return _instance;
            }
        }

        private GameEngineMediator() { }

        public void RegisterGameEngine(IGameEngine gameEngine)
        {
            _gameEngine = gameEngine;
        }

        public void RequestAnimalCreation(IAnimal animal)
        {
            if (_gameEngine != null)
            {
                _gameEngine.AddAnimal(animal);
            }
            else
            {
                // Store for later when the game engine becomes available
                _pendingAnimals.Add(animal);
            }
        }

        public void ProcessPendingAnimals()
        {
            if (_gameEngine != null && _pendingAnimals.Count > 0)
            {
                foreach (var animal in _pendingAnimals)
                {
                    _gameEngine.AddAnimal(animal);
                }
                _pendingAnimals.Clear();
            }
        }
    }
}