using System.Linq.Expressions;

namespace StartTestApi.Data.Repositories
{
    public interface IRepository<T> where T : BaseEntity
    {
        IEnumerable<T> GetAll();
        IEnumerable<T> GetAllByCondition(Expression<Func<T, bool>> predicate);
        T? GetByCondition(Expression<Func<T, bool>> predicate);
        T? GetById(int id);
        void Add(T entity);
        void Update(T entity);
        void Delete(T entity);
        bool SaveChanges();
    }
}
