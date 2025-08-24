using System;
using System.Numerics;

namespace CompactPack.Packers;

public abstract class VariableBitPacker<T, TBitPackerType> : BitPacker<T> where T : struct where TBitPackerType : VariableBitPacker<T, TBitPackerType>, new()
{
    private readonly int _bitLimit;

    public VariableBitPacker()
    {
        _bitLimit = BitLimit();
    }

    internal VariableBitPacker(int bitLimit)
    {
        _bitLimit = bitLimit;
    }

    public new TBitPackerType AddField(string name, int bitWidth)
    {
        if (TotalBitWidth + bitWidth > _bitLimit)
            throw new InvalidOperationException($"Adding field '{name}' with {bitWidth} bits would exceed {_bitLimit}-bit limit. Current usage: {TotalBitWidth} bits");
        base.AddField(name, bitWidth);
        return (TBitPackerType)this;
    }

    public new TBitPackerType AddField(string name, PackRange range)
    {
        var requiredBitWidth = range.BitsRequired;
        if (TotalBitWidth + requiredBitWidth > _bitLimit)
            throw new InvalidOperationException($"Adding field '{name}' would exceed {_bitLimit}-bit limit. Field needs {requiredBitWidth} bits, current usage: {TotalBitWidth} bits");

        base.AddField(name, range);
        return (TBitPackerType)this;
    }


    public new TBitPackerType AddFields(int bitWidth, params string[] names)
    {
        foreach (var name in names)
            AddField(name, bitWidth);
        return (TBitPackerType)this;
    }

    public new TBitPackerType AddFields(PackRange range, params string[] names)
    {
        foreach (var name in names)
            AddField(name, range);
        return (TBitPackerType)this;
    }

    public new TBitPackerType CreateSimilar()
    {
        var newPacker = new TBitPackerType();
        foreach (var field in Fields)
        {
            newPacker.AddFieldInternal(field.Name, field.BitWidth, field.MinValue, field.MaxValue);
        }
        return newPacker;
    }

    public new TBitPackerType Unpack(T packedValue)
    {
        base.Unpack(packedValue);
        return (TBitPackerType)this;
    }

    public new TBitPackerType SetValue(string fieldName, BigInteger value)
    {
        base.SetValue(fieldName, value);
        return (TBitPackerType)this;
    }

    public new TBitPackerType SetValue(string fieldName, int value)
    {
        base.SetValue(fieldName, value);
        return (TBitPackerType)this;
    }

    public new TBitPackerType SetValue(string fieldName, long value)
    {
        base.SetValue(fieldName, value);
        return (TBitPackerType)this;
    }

    public new TBitPackerType AddFieldWithBytes(string name, int byteCount, BigInteger minValue = default)
    {
        var bitWidth = byteCount * 8;
        if (TotalBitWidth + bitWidth > _bitLimit)
            throw new InvalidOperationException($"Adding field '{name}' with {bitWidth} bits would exceed {_bitLimit}-bit limit. Current usage: {TotalBitWidth} bits");
        base.AddFieldWithBytes(name, byteCount, minValue);
        return (TBitPackerType)this;
    }

    /// <summary>
    /// Get the current bit limit for this packer
    /// </summary>
    public int CurrentBitLimit => _bitLimit;
}
