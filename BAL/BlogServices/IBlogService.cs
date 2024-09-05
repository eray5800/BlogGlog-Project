using DAL.Models;
using DAL.Models.DTO.BlogDTO;

namespace BAL.BlogServices
{
    public interface IBlogService
    {
        Task<bool> CreateBlogAsync(BlogDTO blogDto, AppUser user);

        Task<bool> UpdateBlogAsync(Guid blogID, BlogDTO blogDto);

        Task<bool> DeleteBlogAsync(Guid blogId);

        Task<IEnumerable<Blog>> GetAllUserBlogsAsync(string userID);

        Task<IEnumerable<Blog>> GetAllBlogsAsync();

        Task<Blog> GetBlogByIDAsync(Guid blogID);

        Task<IEnumerable<Blog>> GetAllActiveBlogsAsync();

        Task<Blog> GetActiveBlogByIDAsync(Guid blogID);

        Task<IEnumerable<Blog>> Search(string text);

        Task<IEnumerable<Blog>> SearchBlogCategoryAsync(string text);
    }
}
