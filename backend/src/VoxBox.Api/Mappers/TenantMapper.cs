using VoxBox.Api.DTOs;
using VoxBox.Api.Interfaces;
using VoxBox.Core.Entities;

namespace VoxBox.Api.Mappers;

public class TenantMapper : IEntityMapper<Tenant, TenantDto, CreateTenantDto, UpdateTenantDto>
{
    public TenantDto MapToDto(Tenant entity)
    {
        return new TenantDto
        {
            Id = entity.Id,
            Name = entity.Name,
            TenancyName = entity.TenancyName,
            IsPrivate = entity.IsPrivate,
            VoteWeightMode = entity.VoteWeightMode,
            AdminIdentifiers = entity.AdminIdentifiers,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedAt = entity.UpdatedAt,
            ModifiedBy = entity.ModifiedBy
        };
    }

    public Tenant MapToEntity(CreateTenantDto createDto)
    {
        return new Tenant
        {
            Name = createDto.Name,
            TenancyName = createDto.TenancyName,
            IsPrivate = createDto.IsPrivate,
            VoteWeightMode = createDto.VoteWeightMode,
            AdminIdentifiers = createDto.AdminIdentifiers,
            IsActive = createDto.IsActive
        };
    }

    public void MapToEntity(UpdateTenantDto updateDto, Tenant entity)
    {
        entity.Name = updateDto.Name;
        entity.TenancyName = updateDto.TenancyName;
        entity.IsPrivate = updateDto.IsPrivate;
        entity.VoteWeightMode = updateDto.VoteWeightMode;
        entity.AdminIdentifiers = updateDto.AdminIdentifiers;
        entity.IsActive = updateDto.IsActive;
    }
}
