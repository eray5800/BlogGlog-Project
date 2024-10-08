﻿using DAL.Models;
using DAL.Models.DTO.BlogDTO;
using GenericRepoAndUnitOfWork.Core.IRepository;

namespace DAL.Core.IRepository
{
    public interface IBlogRepository : IGenericRepository<Blog, BlogDTO>
    {

        Task<Blog> AddAsync(Blog blog, Category category, AppUser user);

        Task<Blog> UpdateAsync(Blog blog, BlogDTO blogDto, Category category);

        
        Task<IEnumerable<Blog>> SearchAsync(string text);
        Task<IEnumerable<Blog>> SearchBlogCategoryAsync(string Text);

        Task<IEnumerable<Blog>> GetAllUserBlogsByIDAsync(string userID);

        Task<Blog> GetByIDActiveAsync(Guid blogID);

        Task<IEnumerable<Blog>> GetAllActiveAsync();

    }
}
