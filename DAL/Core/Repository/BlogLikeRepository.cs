using DAL.Context;
using DAL.Core.IRepository;
using DAL.Models;
using DAL.Models.DTO.BlogDTO;
using GenericRepoAndUnitOfWork.Core.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DAL.Core.Repository
{
    public class BlogLikeRepository : GenericRepository<BlogLike, BlogLikeDTO>, IBlogLikeRepository
    {
        public BlogLikeRepository(AppIdentityDBContext context, ILogger logger) : base(context, logger)
        {
        }


        public async Task<BlogLike> GetBlogLikeAsync(Guid BlogID, Guid UserID)
        {
            return await dbSet
                .Include(bl => bl.Blog) // Blog ilişkisini dahil etme
                .Include(bl => bl.User) // User ilişkisini dahil etme
                .FirstOrDefaultAsync(bl => bl.Blog.BlogId == BlogID && bl.User.Id == UserID.ToString()) ?? new BlogLike() ;
        }


        public async Task<int> GetBlogLikeCountAsync(Guid BlogID)
        {
            return await dbSet.CountAsync(bl => bl.Blog.BlogId == BlogID);
        }


        public bool DeleteBlogLike(BlogLike blogLike)
        {


            try
            {
                dbSet.Remove(blogLike);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }




        }
    }
}
