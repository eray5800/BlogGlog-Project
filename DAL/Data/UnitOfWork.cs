using DAL.Context;
using DAL.Core.IRepository;
using DAL.Core.Repository;
using GenericRepoAndUnitOfWork.Core.IConfiguration;
using GenericRepoAndUnitOfWork.Core.IRepository;
using GenericRepoAndUnitOfWork.Core.Repository;
using Microsoft.Extensions.Logging;

namespace GenericRepoAndUnitOfWork.Data
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private AppIdentityDBContext context;
        private ILogger logger;



        public ICategoryRepository Categories  { get;private set;}
        public IBlogRepository     Blogs { get; private set; }
        public IBlogTagRepository BlogTags { get; private set; }

        public IWriterRequestRepository WriterRequests { get; private set; }



        public UnitOfWork(AppIdentityDBContext context, ILoggerFactory logger)
        {
            this.context = context;
            this.logger = logger.CreateLogger("ApplicationLogs");

            this.Categories = new CategoryRepository(context,this.logger);
            this.Blogs      = new BlogRepository(context,this.logger);
            this.BlogTags = new BlogTagRepository(context, this.logger);
            this.WriterRequests  = new WriterRequestRepository(context, this.logger);
        }

        public async Task CompleteAsync()
        {
             await context.SaveChangesAsync();
            
        }

        public void Dispose()
        {
            context.Dispose();
        }
    }
}
