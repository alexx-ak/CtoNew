namespace VoxBox.Core.Common;

/// <summary>
/// UUID v7 generator for time-sortable GUIDs using built-in .NET 10 implementation
/// </summary>
public static class GuidGenerator
{
    /// <summary>
    /// Generates a new UUID v7 (time-based, sortable GUID) using Guid.CreateVersion7()
    /// </summary>
    public static Guid GenerateNewGuid() => Guid.CreateVersion7();
    
    /// <summary>
    /// Generates an empty GUID (all zeros)
    /// </summary>
    public static Guid Empty => Guid.Empty;
}