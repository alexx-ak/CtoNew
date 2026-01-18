using System.ComponentModel.DataAnnotations;

namespace VoxBox.Api.DTOs;

public record CreateTenantDto
{
    [Required]
    [StringLength(128, MinimumLength = 1)]
    public string Name { get; init; } = string.Empty;

    [Required]
    [StringLength(64, MinimumLength = 1)]
    public string TenancyName { get; init; } = string.Empty;

    public bool IsPrivate { get; init; } = false;

    public int VoteWeightMode { get; init; } = 0;

    [StringLength(50)]
    public string AdminIdentifiers { get; init; } = string.Empty;

    public bool IsActive { get; init; } = true;
}
