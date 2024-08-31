using DAL.Context;
using DAL.Core.IRepository;
using DAL.Models;
using DAL.Models.DTO.BlogDTO;
using GenericRepoAndUnitOfWork.Core.Repository;
using Microsoft.Extensions.Logging;

namespace DAL.Core.Repository
{
    public class BlogImageRepository : GenericRepository<BlogImage, BlogImageDTO>, IBlogImageRepository
    {
        public BlogImageRepository(AppIdentityDBContext context, ILogger logger) : base(context, logger)
        {
        }


    }
}
