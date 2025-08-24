using System;

namespace CompactPack.Packers;

/// <summary>
/// Specialized bit packer for 64-bit signed integers with automatic overflow checking
/// </summary>
public class Bit64SignedPacker : VariableBitPacker<long, Bit64SignedPacker>
{
    public static Bit64SignedPacker Create() => new();

    public override long Pack()
    {
        long result = 0;
        for (int i = 0; i < FieldCount; i++)
        {
            var field = _fields[i];
            var value = _values[i];
            var normalizedValue = value - field.MinValue;
            long longValue;
            if (normalizedValue < long.MinValue || normalizedValue > long.MaxValue)
            {
                throw new OverflowException($"Value {normalizedValue} is out of long range for field '{field.Name}'");
            }
            longValue = (long)normalizedValue;
            long mask = (1L << field.BitWidth) - 1;
            result |= (longValue & mask) << field.BitOffset;
        }
        return result;
    }

    public override Bit64SignedPacker Unpack(long packedValue)
    {
        for (int i = 0; i < FieldCount; i++)
        {
            var field = _fields[i];
            long mask = (1L << field.BitWidth) - 1;
            long packedFieldValue = (packedValue >> field.BitOffset) & mask;
            _values[i] = packedFieldValue + field.MinValue;
        }
        return this;
    }

}
