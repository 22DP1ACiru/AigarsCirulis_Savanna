using Savanna.Data.Entities;

namespace Savanna.Data.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<GameSave> GameSaves { get; }
        Task<int> CompleteAsync();
    }
}