using DAL.Models;

namespace GenericRepoAndUnitOfWork.Core.IRepository
{
    public interface ICategoryRepository : IGenericRepository<Category,CategoryDTO>
    {
        Task<bool> UpdateAsync(Guid id, CategoryDTO Category);
        Task<IEnumerable<Category>> GetAllActiveAsync();
    }
}
