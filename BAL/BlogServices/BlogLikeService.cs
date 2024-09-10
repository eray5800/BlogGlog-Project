using DAL.Models;
using GenericRepoAndUnitOfWork.Core.IConfiguration;

namespace BAL.BlogServices
{
    public class BlogLikeService : IBlogLikeService
    {
        private readonly IUnitOfWork _unitOfWork;

        public BlogLikeService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BlogLike> GetBlogLikeAsync(Guid BlogID, Guid UserID)
        {
            return await _unitOfWork.BlogLikes.GetBlogLikeAsync(BlogID, UserID);
        }

        public async Task<int> GetBlogLikeCountAsync(Guid BlogID)
        {
            
            return await _unitOfWork.BlogLikes.GetBlogLikeCountAsync(BlogID);
        }

        public async Task<int> AddBlogLikeAsync(BlogLike blogLike)
        {
            var isLikeExists = await GetBlogLikeAsync(blogLike.Blog.BlogId, Guid.Parse(blogLike.User.Id));
            if (isLikeExists.BlogLikeID != Guid.Empty)
            {
                return -1;
            }

            var result = await _unitOfWork.BlogLikes.AddAsync(blogLike);
            if (result != null)
            {
                await _unitOfWork.CompleteAsync();
            }
            var likeCount = await GetBlogLikeCountAsync(blogLike.Blog.BlogId);  
            return likeCount;
        }

        public async Task<int> RemoveBlogLikeAsync(Guid BlogID, Guid userID)
        {
            var blogLike = await _unitOfWork.BlogLikes.GetBlogLikeAsync(BlogID, userID);
            if (blogLike == null || blogLike.User.Id != userID.ToString())
            {
                return -1;
            }

            var result =  _unitOfWork.BlogLikes.DeleteBlogLike(blogLike);
            if (result)
            {
                await _unitOfWork.CompleteAsync();
            }
            var likeCount = await GetBlogLikeCountAsync(blogLike.Blog.BlogId);
            return likeCount;
        }


    }
}
