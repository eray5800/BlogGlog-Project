using DAL.Models;
using DAL.Models.DTO.BlogDTO;
using GenericRepoAndUnitOfWork.Core.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Core.IRepository
{
    public interface IBlogLikeRepository : IGenericRepository<BlogLike, BlogLikeDTO>
    {
        Task<BlogLike> GetBlogLikeAsync(Guid BlogID, Guid UserID);
        Task<int> GetBlogLikeCountAsync(Guid BlogID);

        bool DeleteBlogLike(BlogLike blogLike);

    }
}
