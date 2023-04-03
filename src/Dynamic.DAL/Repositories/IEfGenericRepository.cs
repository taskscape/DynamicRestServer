using Microsoft.EntityFrameworkCore;

namespace Dynamic.DAL.Repositories
{
    public interface IEfGenericRepository<T, out TDbContext> : IGenericRepository<T> where T : class where TDbContext : DbContext
    {
        TDbContext DbContext { get; }
    }
}
