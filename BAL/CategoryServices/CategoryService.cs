using DAL.Models;
using GenericRepoAndUnitOfWork.Core.IConfiguration;

namespace BAL.CategoryServices
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddCategoryAsync(CategoryDTO category)
        {
            bool result = await _unitOfWork.Categories.AddAsync(category);
            if (result)
            {
                await _unitOfWork.CompleteAsync();
            }
            return result;
        }

        public async Task<Category> GetCategoryByIDAsync(Guid categoryId)
        {
            Category category = await _unitOfWork.Categories.GetByIDAsync(categoryId);
            if (category == null)
            {
                return new Category();
            }
            return category;
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            IEnumerable<Category> categories = await _unitOfWork.Categories.GetAllAsync();
            return categories;
        }

        public async Task<IEnumerable<Category>> GetAllActiveCategoriesAsync()
        {
            IEnumerable<Category> categories = await _unitOfWork.Categories.GetAllActiveAsync();
            return categories;
        }

        public async Task<bool> UpdateCategoryAsync(Guid categoryId, CategoryDTO categoryDTO)
        {
            bool result = await _unitOfWork.Categories.UpdateAsync(categoryId, categoryDTO);
            if (result)
            {
                await _unitOfWork.CompleteAsync();
            }
            return result;
        }

        public async Task<bool> DeleteCategoryAsync(Guid categoryId)
        {
            bool result = await _unitOfWork.Categories.DeleteAsync(categoryId);
            if (result)
            {
                await _unitOfWork.CompleteAsync();
            }
            return result;
        }
    }
}
