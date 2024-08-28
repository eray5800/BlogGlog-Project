using DAL.Models;
using GenericRepoAndUnitOfWork.Core.IConfiguration;
using GenericRepoAndUnitOfWork.Core.Repository;
using GenericRepoAndUnitOfWork.Data;
using Microsoft.Extensions.Logging;

namespace BAL
{
    public class CategoryService
    {
        private IUnitOfWork unitOfWork { get; set; }

        public CategoryService(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public async Task<bool> AddCategoryAsync(CategoryDTO category)
        {

            bool result = await unitOfWork.Categories.AddAsync(category);
            if (result)
            {
                await unitOfWork.CompleteAsync();
            }
            return result;
        }

        public async Task<Category> GetCategoryByIDAsync(Guid productID)
        {
            Category category = await unitOfWork.Categories.GetByIDAsync(productID);
            if(category == null)
            {
                return new Category();
            }
            return category;
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            IEnumerable<Category> categories = await unitOfWork.Categories.GetAllAsync();
            return categories;
        }
        
        public async Task<IEnumerable<Category>> GetAllActiveCategoriesAsync()
        {
            IEnumerable<Category> categories = await unitOfWork.Categories.GetAllActiveAsync();
            return categories;
        }

        public async Task<bool> UpdateCategoryAsync(Guid productID, CategoryDTO categoryDTO)
        {
            bool result = await unitOfWork.Categories.UpdateAsync(productID, categoryDTO);
            if (result)
            {
               await unitOfWork.CompleteAsync();
            }
            return result;
        }

        public async Task<bool> DeleteCategoryAsync(Guid productID)
        {
            bool result = await unitOfWork.Categories.DeleteAsync(productID);
            if (result)
            {
                await unitOfWork.CompleteAsync();
            }
            return result;
        }
    }
}
