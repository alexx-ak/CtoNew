namespace VoxBox.Api.DTOs;

public record UserDto
{
    public Guid Id { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string? Name { get; init; }
    public string? Surname { get; init; }
    public string? EmailAddress { get; init; }
    public string? PhoneNumber { get; init; }
    public bool IsActive { get; init; }
    public string? Identifier { get; init; }
    public decimal? VoteWeight { get; init; }
    public string? IdentyumUuid { get; init; }
    public string? PreviousName { get; init; }
    public string? PreviousSurname { get; init; }
    public DateTime CreatedAt { get; init; }
    public Guid? CreatedBy { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public Guid? ModifiedBy { get; init; }
}
