using DAL.Context;
using GenericRepoAndUnitOfWork.Core.IRepository;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace GenericRepoAndUnitOfWork.Core.Repository
{
    public class GenericRepository<T, TDTO> : IGenericRepository<T, TDTO> where T : class
    {
        public AppIdentityDBContext Identitycontext;
        public DbSet<T> dbSet;
        public ILogger logger;



        public GenericRepository(AppIdentityDBContext context, ILogger logger)
        {
            this.Identitycontext = context;
            this.dbSet = context.Set<T>();
            this.logger = logger;
        }
        public virtual Task<bool> AddAsync(TDTO entity)
        {

            throw new NotImplementedException();

        }


        public virtual async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                var entity = await dbSet.FindAsync(id);
                if (entity == null)
                {
                    return false;
                }

                dbSet.Remove(entity);
                // Optionally, save changes outside this method
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting entity.");
                return false;
            }
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> filter)
        {
            return await dbSet.Where(filter).ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await dbSet.ToListAsync();
        }

        public virtual async Task<T> GetByIDAsync(Guid id)
        {
            return await dbSet.FindAsync(id);
        }

        public virtual Task<bool> UpdateAsync(Guid id, TDTO entity)
        {
            throw new NotImplementedException();
        }

    }
}
