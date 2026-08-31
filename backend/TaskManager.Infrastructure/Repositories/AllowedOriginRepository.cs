using TaskManager.Application.Common.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Infrastructure.Data;

namespace TaskManager.Infrastructure.Repositories;

public class AllowedOriginRepository : GenericRepository<AllowedOrigin>, IAllowedOriginRepository
{
    public AllowedOriginRepository(AppDbContext context) : base(context) { }
}
