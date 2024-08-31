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

        public virtual async Task<bool> AddAllAsync(IEnumerable<T> entities)
        {
            try
            {
                await dbSet.AddRangeAsync(entities);
                return true ;
            }
            catch (Exception ex)
            {
                return false;
            }
            
        }
        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            
            return await dbSet.ToListAsync();
        }

        

        public virtual async Task<T> GetByIDAsync(Guid id)
        {
            return await dbSet.FindAsync(id);
        }

        public virtual async Task<bool> DeleteAllAsync(Expression<Func<T, bool>> filter)
        {
            try
            {
                var entities = await dbSet.Where(filter).ToListAsync();
                if (entities == null || !entities.Any())
                {
                    return false;
                }

                dbSet.RemoveRange(entities);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting entities.");
                return false;
            }
        }

        public virtual async Task<bool> AddAsync(T entity)
        {
            await dbSet.AddAsync(entity);
            return true;
        }

        public virtual Task<bool> UpdateAsync(Guid id, TDTO entity)
        {
            throw new NotImplementedException();
        }
    }
}
