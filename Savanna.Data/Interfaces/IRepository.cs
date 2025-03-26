using System.Linq.Expressions;

namespace Savanna.Data.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<T> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> expression);

        void Add(T entity);
        void Update(T entity);
        void Remove(T entity);
    }
}