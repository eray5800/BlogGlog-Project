using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DAL.Core.IRepository;
using DAL.Models;
using DAL.Models.DTO.Blog;
using GenericRepoAndUnitOfWork.Core.IConfiguration;

namespace BAL
{
    public class BlogService
    {
        private IUnitOfWork unitOfWork { get; set; }

        public BlogService(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public async Task<bool> CreateBlogAsync(BlogDTO blogDto, AppUser user)
        {

            Category category = await unitOfWork.Categories.GetByIDAsync(blogDto.SelectedCategoryId);

            if (category == null || !category.IsActive)
            {
                return false;
            }


            var result = await unitOfWork.Blogs.AddAsync(blogDto, category, user);
            if (result)
            {
                await unitOfWork.CompleteAsync();
            }
            return result;
        }

        public async Task<IEnumerable<Blog>> GetAllUserBlogsAsync(string userID)
        {
            var result = await unitOfWork.Blogs.GetAllUserBlogsByIDAsync(userID);
            if (result == null)
            {
                return Enumerable.Empty<Blog>();
            }
            return result;
        }

        public async Task<IEnumerable<Blog>> GetAllBlogsAsync()
        {
            return await unitOfWork.Blogs.GetAllAsync();
        }




        public async Task<Blog> GetBlogByIDAsync(Guid blogID)
        {
            Blog blog = await unitOfWork.Blogs.GetByIDAsync(blogID);
            if (blog == null)
            {
                return new Blog();
            }
            return blog;
        }

        public async Task<bool> UpdateBlogAsync(Guid blogID, BlogDTO blogDto)
        {
            Category category = await unitOfWork.Categories.GetByIDAsync(blogDto.SelectedCategoryId);
            if (category == null || !category.IsActive)
            {
                return false;
            }

            // Blog güncelle
            bool result = await unitOfWork.Blogs.UpdateAsync(blogID, blogDto, category);

            if (result)
            {
                // Güncellenmiş etiketleri işle
                result = await UpdateBlogTagsAsync(blogID, blogDto.BlogTags);

                if (result)
                {
                    await unitOfWork.CompleteAsync();
                }
            }
            return result;
        }


        public async Task<bool> UpdateBlogTagsAsync(Guid blogID, string blogTags)
        {
            // Mevcut blogu ve etiketlerini al
            var existingBlog = await unitOfWork.Blogs.GetByIDAsync(blogID);

            if (existingBlog == null)
            {
                return false;
            }

            // Formdan gelen yeni etiketleri ayır
            var newTagNames = blogTags.Split(',')
                .Select(tag => tag.Trim())
                .ToList();

            // Mevcut tüm etiketleri sil
            await unitOfWork.BlogTags.RemoveAllTagsByBlogIDAsync(blogID);

            // Yeni etiketleri ekle
            var newTags = newTagNames.Select(tag => new BlogTagDTO
            {
                BlogTagID = Guid.NewGuid(),
                TagName = tag,
                Blog = existingBlog
            }).ToList();

            foreach (var tag in newTags)
            {
                await unitOfWork.BlogTags.AddAsync(tag);
            }

            return true;
        }




        public async Task<IEnumerable<Blog>> Search(string Text)
        {
            if (Text == null)
            {
                return new List<Blog>();
            }


            var result = await unitOfWork.Blogs.SearchAsync(Text);
            return result;
        }

        public async Task<IEnumerable<Blog>> SearchBlogCategoryAsync(string Text)
        {
            if (Text == null)
            {
                return new List<Blog>();
            }


            var result = await unitOfWork.Blogs.SearchBlogCategoryAsync(Text);
            return result;
        }
        public async Task<bool> DeleteBlogAsync(Guid blogId)
        {
            bool result = await unitOfWork.Blogs.DeleteAsync(blogId);
            if (result)
            {
                await unitOfWork.CompleteAsync();
            }
            return result;
        }
    }
}
