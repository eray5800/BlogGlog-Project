using DAL.Core.IRepository;
using GenericRepoAndUnitOfWork.Core.IRepository;

namespace GenericRepoAndUnitOfWork.Core.IConfiguration
{
    public interface IUnitOfWork
    {
        ICategoryRepository Categories { get; }
        IBlogRepository Blogs { get; }

        IBlogTagRepository BlogTags { get; }

        IWriterRequestRepository WriterRequests { get; }
        Task CompleteAsync();
    }
}
