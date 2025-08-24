using System;
using System.Numerics;

namespace CompactPack;

/// <summary>
/// Represents a field in the bit-packed structure
/// </summary>
public record BitField(string Name, int BitOffset, int BitWidth, BigInteger MinValue, BigInteger MaxValue)
{
    public BigInteger Mask => (BigInteger.One << BitWidth) - 1;

    public void ValidateValue(BigInteger value)
    {
        if (value < MinValue)
            throw new ArgumentOutOfRangeException(nameof(value), $"Value {value} is below minimum {MinValue} for field '{Name}'");
        if (value > MaxValue)
            throw new ArgumentOutOfRangeException(nameof(value), $"Value {value} exceeds maximum {MaxValue} for field '{Name}'");
    }

    public BigInteger NormalizeValue(BigInteger value)
    {
        ValidateValue(value);
        return value - MinValue; // Shift to 0-based range for packing
    }

    public BigInteger DenormalizeValue(BigInteger packedValue)
    {
        return packedValue + MinValue; // Shift back to original range
    }
}
