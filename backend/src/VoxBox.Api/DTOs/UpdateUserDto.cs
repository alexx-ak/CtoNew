using System.ComponentModel.DataAnnotations;

namespace VoxBox.Api.DTOs;

public record UpdateUserDto
{
    [Required]
    [StringLength(128, MinimumLength = 1)]
    public string UserName { get; init; } = string.Empty;

    [StringLength(128)]
    public string? Name { get; init; }

    [StringLength(128)]
    public string? Surname { get; init; }

    [EmailAddress]
    [StringLength(256)]
    public string? EmailAddress { get; init; }

    [Phone]
    [StringLength(50)]
    public string? PhoneNumber { get; init; }

    public bool IsActive { get; init; }

    [StringLength(128)]
    public string? Identifier { get; init; }

    public decimal? VoteWeight { get; init; }

    [StringLength(128)]
    public string? IdentyumUuid { get; init; }

    [StringLength(128)]
    public string? PreviousName { get; init; }

    [StringLength(128)]
    public string? PreviousSurname { get; init; }
}
