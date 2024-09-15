using DAL.Context;
using DAL.Core.IRepository;
using DAL.Models;
using DAL.Models.DTO.BlogDTO;
using GenericRepoAndUnitOfWork.Core.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DAL.Core.Repository
{
    public class BlogRepository : GenericRepository<Blog, BlogDTO>, IBlogRepository
    {
        public BlogRepository(AppIdentityDBContext context, ILogger logger) : base(context, logger)
        {
        }

        // CREATE
        public async Task<Blog> AddAsync(Blog blog, Category category, AppUser user)
        {
            try
            {
                await dbSet.AddAsync(blog);
                return blog;
            } catch(Exception ex)
            {
                return new Blog();
            }

        }


        public override async Task<Blog> GetByIDAsync(Guid blogID)
        {
            var blog = await dbSet
                .Include(b => b.BlogTags)
                .Include(b => b.Category)
                .Include(b => b.User)
                .Include(b => b.BlogImages) // Include images
                .FirstOrDefaultAsync(x => x.BlogId == blogID);

            return blog;
        }

        public async Task<Blog> GetByIDActiveAsync(Guid blogID)
        {
            var blog = await dbSet
                .Include(b => b.BlogTags)
                .Include(b => b.Category)
                .Include(b => b.User)
                .Include(b => b.BlogImages) // Include images
                .Where(x => x.IsActive)
                .FirstOrDefaultAsync(x => x.BlogId == blogID);

            return blog;
        }


        public override async Task<IEnumerable<Blog>> GetAllAsync()
        {
            try
            {
                return await dbSet
                    .Include(b => b.Category)
                    .Include(b => b.BlogTags)
                    .Include(b => b.User)
                    .Include(b => b.BlogImages)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Blog Repository GetAllAsync Method Error");
                return new List<Blog>();
            }
        }

        public async Task<IEnumerable<Blog>> GetAllActiveAsync()
        {
            try
            {
                return await dbSet
                    .Include(b => b.Category)
                    .Include(b => b.BlogTags)
                    .Include(b => b.User)
                    .Include(b => b.BlogImages)
                    .Where(x => x.IsActive)
                    .ToListAsync() ;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Blog Repository GetAllActiveAsync Method Error");
                return new List<Blog>();
            }
        }

        public async Task<IEnumerable<Blog>> GetAllUserBlogsByIDAsync(string userID)
        {
            return await dbSet
                .Include(b => b.BlogTags)
                .Include(b => b.Category)
                .Include(b => b.User)
                .Include(b => b.BlogImages) // Include images
                .Where(x => x.User.Id == userID)
                .ToListAsync();
        }

        public async Task<Blog> UpdateAsync(Blog existingBlog, BlogDTO blogDto, Category category)
        {
            try
            {
                // Update properties
                existingBlog.BlogTitle = blogDto.BlogTitle;
                existingBlog.Content = blogDto.Content;
                existingBlog.IsActive = blogDto.IsActive;

                // Update Category
                if (category != null)
                {
                    existingBlog.Category = category;
                }


                existingBlog.Updated_At = DateTime.Now;

                return existingBlog;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                logger.LogError(ex, "Concurrency error occurred while updating blog.");
                return null;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while updating the blog.");
                return null;
            }
        }

        // DELETE
        public async Task<bool> DeleteAsync(Guid blogId)
        {
            var blog = await dbSet.FindAsync(blogId);
            if (blog == null) return false;

            dbSet.Remove(blog);
            return true;
        }

        public async Task<IEnumerable<Blog>> SearchAsync(string text)
        {
            try
            {
                return await dbSet
                    .Include(b => b.Category)
                    .Include(b => b.BlogTags)
                    .Include(b => b.User)
                    .Include(b => b.BlogImages) // Include images
                    .Where(x => x.Content.Contains(text) || x.BlogTitle.Contains(text))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Blog Repository SearchAsync Method Error");
                return new List<Blog>();
            }
        }

        public async Task<IEnumerable<Blog>> SearchBlogCategoryAsync(string text)
        {
            try
            {
                return await dbSet
                    .Include(b => b.Category)
                    .Include(b => b.BlogTags)
                    .Include(b => b.User)
                    .Include(b => b.BlogImages) // Include images
                    .Where(b => b.Category.CategoryName == text)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Blog Repository SearchBlogCategoryAsync Method Error");
                return new List<Blog>();
            }
        }
    }
}
