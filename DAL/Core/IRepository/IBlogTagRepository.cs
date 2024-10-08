﻿using DAL.Models;
using DAL.Models.DTO.BlogDTO;
using GenericRepoAndUnitOfWork.Core.IRepository;

namespace DAL.Core.IRepository
{
    public interface IBlogTagRepository : IGenericRepository<BlogTag, BlogTagDTO>
    {
        Task<bool> RemoveAllTagsByBlogIDAsync(Guid blogID);
    }
}
