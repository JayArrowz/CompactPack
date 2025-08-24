using System;

namespace CompactPack.Packers;

/// <summary>
/// Specialized bit packer for 64-bit integers with automatic overflow checking
/// </summary>
public class Bit64Packer : VariableBitPacker<ulong, Bit64Packer>
{
    public static Bit64Packer Create() => new();

    public override ulong Pack()
    {
        ulong result = 0;

        for (int i = 0; i < FieldCount; i++)
        {
            var field = _fields[i];
            var value = _values[i];
            var normalizedValue = value - field.MinValue;
            ulong ulongValue;
            if (normalizedValue < 0 || normalizedValue > ulong.MaxValue)
            {
                throw new OverflowException($"Value {normalizedValue} is out of ulong range for field '{field.Name}'");
            }
            ulongValue = (ulong)normalizedValue;
            ulong mask = (1UL << field.BitWidth) - 1;
            result |= (ulongValue & mask) << field.BitOffset;
        }

        return result;
    }

    public override Bit64Packer Unpack(ulong packedValue)
    {
        for (int i = 0; i < FieldCount; i++)
        {
            var field = _fields[i];
            ulong mask = (1UL << field.BitWidth) - 1;
            ulong packedFieldValue = (packedValue >> field.BitOffset) & mask;
            _values[i] = packedFieldValue + field.MinValue;
        }
        return this;
    }
}
