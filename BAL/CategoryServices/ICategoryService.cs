using DAL.Models;

namespace BAL.CategoryServices
{
    public interface ICategoryService
    {
        Task<bool> AddCategoryAsync(CategoryDTO category);
        Task<Category> GetCategoryByIDAsync(Guid categoryId);
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        Task<IEnumerable<Category>> GetAllActiveCategoriesAsync();
        Task<bool> UpdateCategoryAsync(Guid categoryId, CategoryDTO categoryDTO);
        Task<bool> DeleteCategoryAsync(Guid categoryId);
    }
}
