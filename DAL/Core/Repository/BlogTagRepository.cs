using DAL.Context;
using DAL.Core.IRepository;
using DAL.Models;
using DAL.Models.DTO.BlogDTO;
using GenericRepoAndUnitOfWork.Core.Repository;
using Microsoft.Extensions.Logging;

namespace DAL.Core.Repository
{
    public class BlogTagRepository : GenericRepository<BlogTag, BlogTagDTO>, IBlogTagRepository
    {
        public BlogTagRepository(AppIdentityDBContext context, ILogger logger) : base(context, logger)
        {
        }

        public override async Task<bool> AddAsync(BlogTagDTO entity)
        {
            try
            {
                BlogTag category = new BlogTag()
                {
                    BlogTagID = Guid.NewGuid(),
                    TagName = entity.TagName,
                    Created_At = DateTime.Now,
                    Updated_At = DateTime.Now,
                    IsActive = true,
                    Blog = entity.Blog
                };
                await dbSet.AddAsync(category);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "BlogTag Repository AddAsync Method Error", typeof(CategoryRepository));
                return false;
            }
        }
        public async Task<bool> RemoveAllTagsByBlogIDAsync(Guid blogID)
        {
            // BlogID ile ilişkili tüm BlogTag kayıtlarını al
            var tagsToRemove = dbSet.Where(tag => tag.Blog.BlogId == blogID).ToList();

            if (!tagsToRemove.Any())
            {
                return true;
            }

            // Etiketleri sil
            dbSet.RemoveRange(tagsToRemove);

            return true;

        }

    }
}
