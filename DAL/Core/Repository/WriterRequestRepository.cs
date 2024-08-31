using DAL.Context;
using DAL.Core.IRepository;
using DAL.Models;
using DAL.Models.DTO;
using GenericRepoAndUnitOfWork.Core.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DAL.Core.Repository
{
    public class WriterRequestRepository : GenericRepository<WriterRequest, WriterRequestDTO>, IWriterRequestRepository
    {
        public WriterRequestRepository(AppIdentityDBContext context, ILogger logger) : base(context, logger)
        {
        }

        public async Task<bool> AddAsync(WriterRequestDTO requestDTO, AppUser user)
        {

            WriterRequest wr = new WriterRequest()
            {
                RequestDescription = requestDTO.RequestDescription,
                WriterRequestID = Guid.NewGuid(),
                User = user,
                RequestDate = DateTime.Now,
            };


            await dbSet.AddAsync(wr);
            return true;
        }


        // READ - Get All
        public override async Task<IEnumerable<WriterRequest>> GetAllAsync()
        {
            try
            {

                return await dbSet
                    .Include(b => b.User)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "WriterRequest Repository GetAllAsync Method Error", typeof(CategoryRepository));
                return new List<WriterRequest>();
            }
        }


        public override async Task<WriterRequest> GetByIDAsync(Guid requestID)
        {

            var writerRequest = await dbSet
                      .Include(b => b.User)
                      .FirstOrDefaultAsync(x => x.WriterRequestID == requestID);

            if (writerRequest == null) return new WriterRequest();

            return writerRequest;
        }

        public override Task<bool> DeleteAsync(Guid id)
        {
            return base.DeleteAsync(id);
        }

    }
}
