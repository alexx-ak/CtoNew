namespace VoxBox.Core.Common;

/// <summary>
/// UUID v7 generator for time-sortable GUIDs
/// </summary>
public static class GuidGenerator
{
    private static readonly Random Random = new();
    
    /// <summary>
    /// Generates a new UUID v7 (time-based, sortable GUID)
    /// </summary>
    public static Guid GenerateNewGuid()
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var guid = new byte[16];
        
        // Write timestamp in milliseconds (48 bits for unix_ts_ms)
        guid[0] = (byte)(timestamp >> 40);
        guid[1] = (byte)(timestamp >> 32);
        guid[2] = (byte)(timestamp >> 24);
        guid[3] = (byte)(timestamp >> 16);
        guid[4] = (byte)(timestamp >> 8);
        guid[5] = (byte)timestamp;
        
        // Version (4 bits) = 7
        // Variant (2 bits) = 10
        // randomness (62 bits)
        guid[6] = (byte)(0b01110000 | (Random.Next(0, 16))); // ver 7 + 4 random bits
        guid[7] = (byte)Random.Next(0, 256); // 8 random bits
        guid[8] = (byte)(0b10000000 | (Random.Next(0, 64))); // variant 10 + 6 random bits
        
        for (var i = 9; i < 16; i++)
        {
            guid[i] = (byte)Random.Next(0, 256); // 56 more random bits
        }
        
        return new Guid(guid);
    }
    
    /// <summary>
    /// Generates an empty GUID (all zeros)
    /// </summary>
    public static Guid Empty => Guid.Empty;
}