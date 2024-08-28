using System.Linq.Expressions;

namespace GenericRepoAndUnitOfWork.Core.IRepository
{
    public interface IGenericRepository<T,TDTO> where T: class
    {
        Task<IEnumerable<T>> GetAllAsync();

        Task<T> GetByIDAsync(Guid id);

        Task<bool> AddAsync(TDTO entity);

        Task<bool> UpdateAsync(Guid id, TDTO entity);
        Task<bool> DeleteAsync(Guid id);

        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> filter);




    }
}
