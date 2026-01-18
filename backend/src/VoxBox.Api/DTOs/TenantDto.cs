namespace VoxBox.Api.DTOs;

public class TenantDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string TenancyName { get; set; } = string.Empty;
    public bool IsPrivate { get; set; }
    public int VoteWeightMode { get; set; }
    public string AdminIdentifiers { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? ModifiedBy { get; set; }
}
