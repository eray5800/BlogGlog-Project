using DAL.Models;
using DAL.Models.DTO;
using GenericRepoAndUnitOfWork.Core.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Core.IRepository
{
    public interface IWriterRequestRepository : IGenericRepository<WriterRequest, WriterRequestDTO>
    {
        Task<bool> AddAsync(WriterRequestDTO requestDTO, AppUser user);
    }
}
