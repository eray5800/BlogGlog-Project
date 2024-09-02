using DAL.Models;
using DAL.Models.DTO;
using GenericRepoAndUnitOfWork.Core.IConfiguration;
using Microsoft.Extensions.Logging;

namespace BAL.Writer.WriterRequestServices
{
    public class WriterRequestService : IWriterRequestService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ILogger<WriterRequestService> logger;

        public WriterRequestService(IUnitOfWork unitOfWork, ILogger<WriterRequestService> logger)
        {
            this.unitOfWork = unitOfWork;
            this.logger = logger;
        }

        public async Task<bool> AddWriterRequestAsync(WriterRequestDTO requestDTO, AppUser user)
        {
            try
            {
                bool result = await unitOfWork.WriterRequests.AddAsync(requestDTO, user);
                if (result)
                {
                    await unitOfWork.CompleteAsync();
                }
                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in AddWriterRequestAsync");
                return false;
            }
        }

        public async Task<WriterRequest> GetWriterRequestByIDAsync(Guid requestID)
        {
            try
            {
                WriterRequest writerRequest = await unitOfWork.WriterRequests.GetByIDAsync(requestID);
                return writerRequest;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in GetWriterRequestByIDAsync");
                return null;
            }
        }

        public async Task<IEnumerable<WriterRequest>> GetAllWriterRequestsAsync()
        {
            try
            {
                return await unitOfWork.WriterRequests.GetAllAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in GetAllWriterRequestsAsync");
                return new List<WriterRequest>();
            }
        }

        public async Task<bool> DeleteWriterRequestAsync(Guid requestID)
        {
            try
            {
                bool result = await unitOfWork.WriterRequests.DeleteAsync(requestID);
                if (result)
                {
                    await unitOfWork.CompleteAsync();
                }
                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in DeleteWriterRequestAsync");
                return false;
            }
        }
    }
}
