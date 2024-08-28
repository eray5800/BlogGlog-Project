using DAL.Context;
using DAL.Models;
using GenericRepoAndUnitOfWork.Core.IRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GenericRepoAndUnitOfWork.Core.Repository
{
    public class CategoryRepository : GenericRepository<Category, CategoryDTO>, ICategoryRepository
    {
        public CategoryRepository(AppIdentityDBContext context,ILogger logger ) : base( context, logger ) {
        
        
        }

        public override async Task<bool> AddAsync(CategoryDTO entity)
        {
            try
            {
                Category category = new Category()
                {
                    CategoryID = Guid.NewGuid(),
                    CategoryName = entity.CategoryName,
                    Created_At = DateTime.Now,
                    Updated_At = DateTime.Now,
                    IsActive = entity.IsActive,
                };
                await dbSet.AddAsync(category);
                return true;
            }
            catch (Exception ex) {
                logger.LogError(ex, "Category Repository AddAsync Method Error", typeof(CategoryRepository));
                return false;
            }
        }

        public override async Task<IEnumerable<Category>> GetAllAsync()
        {
            try
            {
                return await dbSet.ToListAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Category Repository GetAllAsync Method Error", typeof(CategoryRepository));
                return new List<Category>();
            }
            
        }

        public async Task<IEnumerable<Category>> GetAllActiveAsync()
        {
            try
            {
                return await dbSet.Where(x => x.IsActive == true).ToListAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Category Repository GetAllAsync Method Error", typeof(CategoryRepository));
                return new List<Category>();
            }
        }

    public override async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                Category user = await dbSet.SingleOrDefaultAsync(x => x.CategoryID == id);
                if (user == null)
                {
                    return false;
                }

                dbSet.Remove(user);
                return true;

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Category Repository DeleteAsync Method Error", typeof(CategoryRepository));
                return false;

            }
        }

        public override async Task<bool> UpdateAsync(Guid id, CategoryDTO entity)
        {
            try
            {
                Category category = dbSet.FirstOrDefault(x => x.CategoryID == id);
                if (category == null)
                {
                    return false;
                }
                category.IsActive = entity.IsActive;
                category.Updated_At = DateTime.Now;
                category.CategoryName = entity.CategoryName;

                dbSet.Update(category);
                return true;
               
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Category Repository UpdateAsync Method Error", typeof(CategoryRepository));
                return false;
            }
        }
    }
}
