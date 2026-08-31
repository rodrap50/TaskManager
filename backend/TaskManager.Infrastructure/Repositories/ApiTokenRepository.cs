using TaskManager.Application.Common.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Infrastructure.Data;

namespace TaskManager.Infrastructure.Repositories;

public class ApiTokenRepository : GenericRepository<ApiToken>, IApiTokenRepository
{
    public ApiTokenRepository(AppDbContext context) : base(context) { }
}
