using VoxBox.Core.Common;

namespace VoxBox.Core.Entities;

/// <summary>
/// User entity representing a user in the system.
/// </summary>
public class User : BaseEntityLong
{
    public string UserName { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Surname { get; set; }
    public string? EmailAddress { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Identifier { get; set; }
    public decimal? VoteWeight { get; set; }
    public string? IdentyumUuid { get; set; }
    public string? PreviousName { get; set; }
    public string? PreviousSurname { get; set; }
}