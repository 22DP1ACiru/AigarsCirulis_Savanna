using Savanna.Data.Entities;
using Savanna.Data.Interfaces;

namespace Savanna.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IRepository<GameSave> _gameSaves;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IRepository<GameSave> GameSaves =>
            _gameSaves ??= new Repository<GameSave>(_context);

        public int Complete() => _context.SaveChanges();

        public void Dispose() => _context.Dispose();
    }
}