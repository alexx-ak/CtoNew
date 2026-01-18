namespace VoxBox.Api.DTOs;

public record TenantDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string TenancyName { get; init; } = string.Empty;
    public bool IsPrivate { get; init; }
    public int VoteWeightMode { get; init; }
    public string AdminIdentifiers { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public Guid? CreatedBy { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public Guid? ModifiedBy { get; init; }
}
