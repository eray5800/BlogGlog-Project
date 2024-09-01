using BAL.CategoryServices;
using DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogProjeAPI.Controllers.Admin
{
    [Route("api/admin/category/[action]")]
    [ApiController]

    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return Ok(categories);
        }

        [HttpGet]
        [Authorize]

        public async Task<IActionResult> GetAllActiveCategories()
        {
            var categories = await _categoryService.GetAllActiveCategoriesAsync();
            return Ok(categories);
        }

        [HttpGet("{productID}")]


        [Authorize(Roles = "Admin")]

        public async Task<Category> GetCategoryByID(Guid productID)
        {
            Category category = await _categoryService.GetCategoryByIDAsync(productID);

            return category;

        }

        [HttpPost]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> AddCategory([FromBody] CategoryDTO categoryDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _categoryService.AddCategoryAsync(categoryDto);

            if (!result)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error adding the category.");
            }

            return Ok("Category added successfully.");
        }

        [HttpPut("{productID}")]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> UpdateCategory(Guid productID, [FromBody] CategoryDTO categoryDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _categoryService.UpdateCategoryAsync(productID, categoryDto);

            if (!result)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error updating the category.");
            }

            return Ok("Category updated successfully.");
        }

        [HttpDelete("{productID}")]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> DeleteCategory(Guid productID)
        {
            var result = await _categoryService.DeleteCategoryAsync(productID);

            if (!result)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error deleting the category.");
            }

            return Ok("Category deleted successfully.");
        }
    }
}
