using System;

namespace CompactPack.Packers;

/// <summary>
/// Specialized bit packer for 32-bit integers with automatic overflow checking
/// </summary>
public class Bit32Packer : VariableBitPacker<uint, Bit32Packer>
{
    public static Bit32Packer Create() => new();

    public override uint Pack()
    {
        uint result = 0;

        for (int i = 0; i < FieldCount; i++)
        {
            var field = _fields[i];
            var value = _values[i];
            var normalizedValue = value - field.MinValue;
            uint uintValue;
            if (normalizedValue < 0 || normalizedValue > uint.MaxValue)
            {
                throw new OverflowException($"Value {normalizedValue} is out of uint range for field '{field.Name}'");
            }
            uintValue = (uint)normalizedValue;

            // Create mask and apply
            uint mask = (1u << field.BitWidth) - 1;
            result |= (uintValue & mask) << field.BitOffset;
        }
        return result;
    }

    public override Bit32Packer Unpack(uint packedValue)
    {
        for (int i = 0; i < FieldCount; i++)
        {
            var field = _fields[i];
            uint mask = (1u << field.BitWidth) - 1;
            uint packedFieldValue = (packedValue >> field.BitOffset) & mask;
            _values[i] = packedFieldValue + field.MinValue;
        }
        return this;
    }
}