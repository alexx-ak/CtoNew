using Microsoft.AspNetCore.Mvc;
using VoxBox.Api.DTOs;
using VoxBox.Core.Entities;
using VoxBox.Infrastructure.Persistence;

namespace VoxBox.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TenantsController(VoxBoxDbContext dbContext) : ControllerBase
{
    private readonly VoxBoxDbContext _dbContext = dbContext;
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TenantDto>>> GetAll(CancellationToken cancellationToken)
    {
        var repository = _dbContext.GetRepository<Tenant>();
        var tenants = await repository.GetAllAsync(includeDeleted: false, cancellationToken);
        var tenantDtos = tenants.Select(t => new TenantDto
        {
            Id = t.Id,
            Name = t.Name,
            TenancyName = t.TenancyName,
            IsPrivate = t.IsPrivate,
            VoteWeightMode = t.VoteWeightMode,
            AdminIdentifiers = t.AdminIdentifiers,
            IsActive = t.IsActive,
            CreatedAt = t.CreatedAt,
            CreatedBy = t.CreatedBy,
            UpdatedAt = t.UpdatedAt,
            ModifiedBy = t.ModifiedBy
        });

        return Ok(tenantDtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TenantDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var repository = _dbContext.GetRepository<Tenant>();
        var tenant = await repository.GetByIdAsync(id, includeDeleted: false, cancellationToken);
        if (tenant == null)
        {
            return NotFound(new { message = $"Tenant with ID {id} not found" });
        }

        var tenantDto = new TenantDto
        {
            Id = tenant.Id,
            Name = tenant.Name,
            TenancyName = tenant.TenancyName,
            IsPrivate = tenant.IsPrivate,
            VoteWeightMode = tenant.VoteWeightMode,
            AdminIdentifiers = tenant.AdminIdentifiers,
            IsActive = tenant.IsActive,
            CreatedAt = tenant.CreatedAt,
            CreatedBy = tenant.CreatedBy,
            UpdatedAt = tenant.UpdatedAt,
            ModifiedBy = tenant.ModifiedBy
        };

        return Ok(tenantDto);
    }

    [HttpPost]
    public async Task<ActionResult<TenantDto>> Create([FromBody] CreateTenantDto createDto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var repository = _dbContext.GetRepository<Tenant>();
        var tenant = new Tenant
        {
            Name = createDto.Name,
            TenancyName = createDto.TenancyName,
            IsPrivate = createDto.IsPrivate,
            VoteWeightMode = createDto.VoteWeightMode,
            AdminIdentifiers = createDto.AdminIdentifiers,
            IsActive = createDto.IsActive
        };

        var createdTenant = await repository.AddAsync(tenant, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var tenantDto = new TenantDto
        {
            Id = createdTenant.Id,
            Name = createdTenant.Name,
            TenancyName = createdTenant.TenancyName,
            IsPrivate = createdTenant.IsPrivate,
            VoteWeightMode = createdTenant.VoteWeightMode,
            AdminIdentifiers = createdTenant.AdminIdentifiers,
            IsActive = createdTenant.IsActive,
            CreatedAt = createdTenant.CreatedAt,
            CreatedBy = createdTenant.CreatedBy,
            UpdatedAt = createdTenant.UpdatedAt,
            ModifiedBy = createdTenant.ModifiedBy
        };

        return CreatedAtAction(nameof(GetById), new { id = tenantDto.Id }, tenantDto);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TenantDto>> Update(Guid id, [FromBody] UpdateTenantDto updateDto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var repository = _dbContext.GetRepository<Tenant>();
        var existingTenant = await repository.GetByIdAsync(id, includeDeleted: false, cancellationToken);
        if (existingTenant == null)
        {
            return NotFound(new { message = $"Tenant with ID {id} not found" });
        }

        existingTenant.Name = updateDto.Name;
        existingTenant.TenancyName = updateDto.TenancyName;
        existingTenant.IsPrivate = updateDto.IsPrivate;
        existingTenant.VoteWeightMode = updateDto.VoteWeightMode;
        existingTenant.AdminIdentifiers = updateDto.AdminIdentifiers;
        existingTenant.IsActive = updateDto.IsActive;

        await repository.UpdateAsync(existingTenant, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var tenantDto = new TenantDto
        {
            Id = existingTenant.Id,
            Name = existingTenant.Name,
            TenancyName = existingTenant.TenancyName,
            IsPrivate = existingTenant.IsPrivate,
            VoteWeightMode = existingTenant.VoteWeightMode,
            AdminIdentifiers = existingTenant.AdminIdentifiers,
            IsActive = existingTenant.IsActive,
            CreatedAt = existingTenant.CreatedAt,
            CreatedBy = existingTenant.CreatedBy,
            UpdatedAt = existingTenant.UpdatedAt,
            ModifiedBy = existingTenant.ModifiedBy
        };

        return Ok(tenantDto);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var repository = _dbContext.GetRepository<Tenant>();
        var existingTenant = await repository.GetByIdAsync(id, includeDeleted: false, cancellationToken);
        if (existingTenant == null)
        {
            return NotFound(new { message = $"Tenant with ID {id} not found" });
        }

        await repository.DeleteAsync(id, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}
