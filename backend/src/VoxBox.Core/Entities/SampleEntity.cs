using VoxBox.Core.Common;

namespace VoxBox.Core.Entities;

/// <summary>
/// Sample entity demonstrating Code First approach following SOLID:
/// - Single Responsibility: Represents a domain entity
/// - KISS: Simple, focused class with minimal properties
/// </summary>
public class SampleEntity : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
