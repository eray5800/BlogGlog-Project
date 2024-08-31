using DAL.Models;
using DAL.Models.DTO;
using GenericRepoAndUnitOfWork.Core.IRepository;

namespace DAL.Core.IRepository
{
    public interface IWriterRequestRepository : IGenericRepository<WriterRequest, WriterRequestDTO>
    {
        Task<bool> AddAsync(WriterRequestDTO requestDTO, AppUser user);
    }
}
