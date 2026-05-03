using System.Linq.Expressions;
using api.data.Entities;
using api.Helper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace api.data.Repositories
{
    public class BaseRepository<TEntity>(ApplicationContext dbContextFactory) : IBaseRepository<TEntity> where TEntity : BaseEntity
    {
        protected readonly ApplicationContext _dbContext = dbContextFactory;

        public virtual async Task<IList<TEntity>> GetAll()
        {
            return await _dbContext.Set<TEntity>()
                .Where(x => x.Enable)
                .ToListAsync();
        }

        public async Task<IEnumerable<TEntity>> InsertMany(IEnumerable<TEntity> entities)
        {
            ArgumentNullException.ThrowIfNull(entities);

            if (!entities.Any())
            {
                return [];
            }

            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var utcNow = DateTime.UtcNow;

                foreach (var entity in entities)
                {
                    if (entity == null)
                        throw new ArgumentException("Entity cannot be null.", nameof(entities));

                    if (entity.CreatedAt == default)
                        entity.CreatedAt = utcNow;

                    entity.Enable = true;
                    entity.UpdatedAt ??= utcNow;
                }

                await _dbContext.Set<TEntity>().AddRangeAsync(entities);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return entities;
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException(
                    $"Failed to save entities due to a database error: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException("An unexpected error occurred while saving entities.", ex);
            }
        }

        public virtual async Task<TEntity?> Get(int id)
        {
            return await _dbContext.Set<TEntity>()
                .Where(x => x.Enable)
                .FirstOrDefaultAsync(x => x.Id.Equals(id));
        }

        public async Task<TEntity> Create(TEntity obj)
        {
            EntityEntry<TEntity> newObj = await _dbContext.Set<TEntity>().AddAsync(obj);
            newObj.Entity.CreatedAt = DateTime.Now.ToUniversalTime();
            await _dbContext.SaveChangesAsync();
            return newObj.Entity;
        }

        public async Task<TEntity> Update(TEntity obj)
        {
            EntityEntry<TEntity> newObj = _dbContext.Set<TEntity>().Update(obj);
            newObj.Entity.UpdatedAt = DateTime.Now.ToUniversalTime();
            await _dbContext.SaveChangesAsync();
            return newObj.Entity;
        }

        public async Task Delete(int id)
        {
            TEntity? obj = await _dbContext.Set<TEntity>().FirstOrDefaultAsync(x => x.Id.Equals(id));
            if (obj != null)
            {
                obj.DeletedAt = DateTime.Now.ToUniversalTime();
                obj.Enable = false;
                await _dbContext.SaveChangesAsync();
            }
        }

        public IQueryable<TEntity> GetAllAsQueryable()
        {
            return _dbContext.Set<TEntity>()
                .Where(x => x.Enable)
                .AsQueryable();
        }

        public async Task<PaginatedResult<TEntity>> GetPaged(
            QueryParams queryParams,
            Func<IQueryable<TEntity>, string, IQueryable<TEntity>>? searchFilter = null)
        {
            var query = GetAllAsQueryable();

            // Apply search
            if (!string.IsNullOrWhiteSpace(queryParams.Search) && searchFilter != null)
                query = searchFilter(query, queryParams.Search.Trim());

            // Apply sorting via reflection — falls back to Id if property not found
            if (!string.IsNullOrWhiteSpace(queryParams.SortBy))
            {
                var property = typeof(TEntity).GetProperty(
                    queryParams.SortBy,
                    System.Reflection.BindingFlags.IgnoreCase |
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.Instance);

                if (property != null)
                {
                    var param = Expression.Parameter(typeof(TEntity), "e");
                    var member = Expression.Property(param, property);
                    var converted = Expression.Convert(member, typeof(object));
                    var keySelector = Expression.Lambda<Func<TEntity, object>>(converted, param);

                    query = queryParams.SortDesc
                        ? query.OrderByDescending(keySelector)
                        : query.OrderBy(keySelector);
                }
                else
                {
                    query = query.OrderBy(e => e.Id);
                }
            }
            else
            {
                query = query.OrderBy(e => e.Id);
            }

            return await PaginationHelper.CreateAsync(query, queryParams.PageNumber, queryParams.PageSize);
        }
    }
}
