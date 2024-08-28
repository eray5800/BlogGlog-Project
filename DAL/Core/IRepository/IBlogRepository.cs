using DAL.Models;
using DAL.Models.DTO.Blog;
using GenericRepoAndUnitOfWork.Core.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Core.IRepository
{
    public interface IBlogRepository : IGenericRepository<Blog,BlogDTO>
    {

        Task<bool> AddAsync(BlogDTO blogDto, Category category,AppUser user);

        Task<bool> UpdateAsync(Guid blogID, BlogDTO blogDto, Category category);
        Task<IEnumerable<Blog>> SearchAsync(string text);
        Task<IEnumerable<Blog>> SearchBlogCategoryAsync(string Text);

        Task<IEnumerable<Blog>> GetAllUserBlogsByIDAsync(string userID);

    }
}
