using System;
using System.Numerics;

namespace CompactPack;

/// <summary>
/// Helper class for calculating bit requirements
/// </summary>
public static class BitHelper
{
    /// <summary>
    /// Calculate minimum bits needed to store a range of values
    /// </summary>
    public static int BitsForRange(BigInteger min, BigInteger max)
    {
        if (max < min)
            throw new ArgumentException("Max value cannot be less than min value");

        var range = max - min;
        if (range == 0) return 1; // Need at least 1 bit

        return (int)Math.Ceiling(BigInteger.Log(range + 1, 2));
    }

    /// <summary>
    /// Calculate minimum bits needed for unsigned values up to max
    /// </summary>
    public static int BitsForValue(BigInteger maxValue)
    {
        return BitsForRange(0, maxValue);
    }

    /// <summary>
    /// Calculate minimum bytes needed for a bit count
    /// </summary>
    public static int BytesForBits(int bitCount)
    {
        return (bitCount + 7) / 8; // Ceiling division
    }
}