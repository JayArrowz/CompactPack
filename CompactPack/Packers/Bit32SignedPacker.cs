using System;

namespace CompactPack.Packers;

/// <summary>
/// Specialized bit packer for 32-bit signed integers with automatic overflow checking
/// </summary>
public class Bit32SignedPacker : VariableBitPacker<int, Bit32SignedPacker>
{
    public static Bit32SignedPacker Create() => new();

    public override int Pack()
    {
        int result = 0;

        for (int i = 0; i < FieldCount; i++)
        {
            var field = _fields[i];
            var value = _values[i];
            var normalizedValue = value - field.MinValue;
            int intValue;
            if (normalizedValue < int.MinValue || normalizedValue > int.MaxValue)
            {
                throw new OverflowException($"Value {normalizedValue} is out of int range for field '{field.Name}'");
            }
            intValue = (int)normalizedValue;
            int mask = (1 << field.BitWidth) - 1;
            result |= (intValue & mask) << field.BitOffset;
        }
        return result;
    }

    public override Bit32SignedPacker Unpack(int packedValue)
    {

        for (int i = 0; i < FieldCount; i++)
        {
            var field = _fields[i];
            int mask = (1 << field.BitWidth) - 1;
            int packedFieldValue = (packedValue >> field.BitOffset) & mask;
            _values[i] = packedFieldValue + field.MinValue;
        }
        return this;
    }

}
