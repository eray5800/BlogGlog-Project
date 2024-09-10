using DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BAL.BlogServices
{
    public interface IBlogLikeService
    {
        Task<BlogLike> GetBlogLikeAsync(Guid BlogID, Guid UserID);

        Task<int> GetBlogLikeCountAsync(Guid BlogID);

        Task<int> AddBlogLikeAsync(BlogLike blogLike); 

        Task<int> RemoveBlogLikeAsync(Guid BlogLikeID, Guid userID);



    }

}
