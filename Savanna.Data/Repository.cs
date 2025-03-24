using Microsoft.EntityFrameworkCore;
using Savanna.Data.Interfaces;
using System.Linq.Expressions;

namespace Savanna.Data
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
        }

        public void Add(T entity) => _context.Set<T>().Add(entity);
        public void Update(T entity) => _context.Set<T>().Update(entity);
        public void Remove(T entity) => _context.Set<T>().Remove(entity);

        public T GetById(int id) => _context.Set<T>().Find(id);

        public IEnumerable<T> GetAll() => _context.Set<T>().ToList();

        public IEnumerable<T> Find(Expression<Func<T, bool>> expression) =>
            _context.Set<T>().Where(expression).ToList();
    }
}