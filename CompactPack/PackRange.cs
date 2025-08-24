using System;
using System.Numerics;

namespace CompactPack;

/// <summary>
/// Represents an inclusive range of values for bit packing
/// </summary>
public readonly struct PackRange
{
    public BigInteger Min { get; }
    public BigInteger Max { get; }

    private PackRange(BigInteger min, BigInteger max)
    {
        if (max < min)
            throw new ArgumentException("Max value cannot be less than min value");
        Min = min;
        Max = max;
    }

    /// <summary>
    /// Create a range from 0 to maxValue (inclusive)
    /// </summary>
    public static PackRange Of(BigInteger maxValue) => new(0, maxValue);

    /// <summary>
    /// Create a range from 0 to maxValue (inclusive) - int overload
    /// </summary>
    public static PackRange Of(int maxValue) => new(0, maxValue);

    /// <summary>
    /// Create a range from 0 to maxValue (inclusive) - long overload
    /// </summary>
    public static PackRange Of(long maxValue) => new(0, maxValue);

    /// <summary>
    /// Create a range from minValue to maxValue (inclusive)
    /// </summary>
    public static PackRange Of(BigInteger minValue, BigInteger maxValue) => new(minValue, maxValue);

    /// <summary>
    /// Create a range from minValue to maxValue (inclusive) - int overload
    /// </summary>
    public static PackRange Of(int minValue, int maxValue) => new(minValue, maxValue);

    /// <summary>
    /// Create a range from minValue to maxValue (inclusive) - long overload
    /// </summary>
    public static PackRange Of(long minValue, long maxValue) => new(minValue, maxValue);

    /// <summary>
    /// Get the span of the range (max - min)
    /// </summary>
    public BigInteger Span => Max - Min;

    /// <summary>
    /// Get the number of distinct values in this range
    /// </summary>
    public BigInteger ValueCount => Span + 1;

    /// <summary>
    /// Get the minimum bits needed to represent all values in this range
    /// </summary>
    public int BitsRequired => BitHelper.BitsForRange(Min, Max);

    /// <summary>
    /// Check if a value is within this range
    /// </summary>
    public bool Contains(BigInteger value) => value >= Min && value <= Max;

    /// <summary>
    /// Check if a value is within this range - int overload
    /// </summary>
    public bool Contains(int value) => Contains(new BigInteger(value));

    /// <summary>
    /// Check if a value is within this range - long overload
    /// </summary>
    public bool Contains(long value) => Contains(new BigInteger(value));

    public override string ToString() => $"[{Min}..{Max}]";

    public override bool Equals(object obj) => obj is PackRange other && Min == other.Min && Max == other.Max;

    public override int GetHashCode() => HashCode.Combine(Min, Max);

    public static bool operator ==(PackRange left, PackRange right) => left.Equals(right);

    public static bool operator !=(PackRange left, PackRange right) => !left.Equals(right);
}
