using System.ComponentModel.DataAnnotations;

namespace VoxBox.Api.DTOs;

public record UpdateTenantDto
{
    [Required]
    [StringLength(128, MinimumLength = 1)]
    public string Name { get; init; } = string.Empty;

    [Required]
    [StringLength(64, MinimumLength = 1)]
    public string TenancyName { get; init; } = string.Empty;

    public bool IsPrivate { get; init; }

    public int VoteWeightMode { get; init; }

    [StringLength(50)]
    public string AdminIdentifiers { get; init; } = string.Empty;

    public bool IsActive { get; init; }
}
