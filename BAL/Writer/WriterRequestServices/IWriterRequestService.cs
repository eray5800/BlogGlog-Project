using DAL.Models.DTO;
using DAL.Models;

namespace BAL.Writer.WriterRequestServices
{
    public interface IWriterRequestService
    {
        Task<bool> AddWriterRequestAsync(WriterRequestDTO requestDTO, AppUser user);

        Task<WriterRequest> GetWriterRequestByIDAsync(Guid requestID);

        Task<IEnumerable<WriterRequest>> GetAllWriterRequestsAsync();

        Task<bool> DeleteWriterRequestAsync(Guid requestID);
    }
}
