using VoxBox.Api.DTOs;
using VoxBox.Api.Mappers;
using VoxBox.Core.Entities;
using VoxBox.Infrastructure.Persistence;

namespace VoxBox.Api.Controllers;

public class TenantsController(VoxBoxDbContext dbContext, TenantMapper mapper)
    : BaseController<Tenant, TenantDto, CreateTenantDto, UpdateTenantDto>(dbContext, mapper)
{
}
