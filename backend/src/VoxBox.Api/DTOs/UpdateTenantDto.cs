using System.ComponentModel.DataAnnotations;

namespace VoxBox.Api.DTOs;

public class UpdateTenantDto
{
    [Required]
    [StringLength(128, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(64, MinimumLength = 1)]
    public string TenancyName { get; set; } = string.Empty;

    public bool IsPrivate { get; set; }

    public int VoteWeightMode { get; set; }

    [StringLength(50)]
    public string AdminIdentifiers { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}
