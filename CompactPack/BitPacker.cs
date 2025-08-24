using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace CompactPack;

/// <summary>
/// Generic bit packer for packing multiple values into a single integer type
/// </summary>
public class BitPacker<T> where T : struct
{
    internal readonly List<BitField> _fields = new();
    private readonly Dictionary<string, BigInteger> _values = new();
    private int _currentBitOffset = 0;

    /// <summary>
    /// Add a field with explicit bit width
    /// </summary>
    public BitPacker<T> AddField(string name, int bitWidth)
    {
        return AddFieldInternal(name, bitWidth, 0, (BigInteger.One << bitWidth) - 1);
    }

    /// <summary>
    /// Add a field with a specific value range
    /// </summary>
    public BitPacker<T> AddField(string name, PackRange range)
    {
        var bitWidth = range.BitsRequired;
        return AddFieldInternal(name, bitWidth, range.Min, range.Max);
    }

    /// <summary>
    /// Add a field optimized for a specific byte count (for when you know exact storage needs)
    /// </summary>
    public BitPacker<T> AddFieldWithBytes(string name, int byteCount, BigInteger minValue = default)
    {
        var bitWidth = byteCount * 8;
        var maxValue = minValue + (BigInteger.One << bitWidth) - 1;
        return AddFieldInternal(name, bitWidth, minValue, maxValue);
    }

    /// <summary>
    /// Add multiple fields with the same bit width
    /// </summary>
    public BitPacker<T> AddFields(int bitWidth, params string[] names)
    {
        foreach (var name in names)
            AddField(name, bitWidth);
        return this;
    }

    /// <summary>
    /// Add multiple fields with the same value range
    /// </summary>
    public BitPacker<T> AddFields(PackRange range, params string[] names)
    {
        foreach (var name in names)
            AddField(name, range);
        return this;
    }

    internal BitPacker<T> AddFieldInternal(string name, int bitWidth, BigInteger minValue, BigInteger maxValue)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Field name cannot be null or empty", nameof(name));
        if (bitWidth <= 0)
            throw new ArgumentOutOfRangeException(nameof(bitWidth), "Bit width must be positive");
        if (Fields.Any(f => f.Name == name))
            throw new ArgumentException($"Field '{name}' already exists", nameof(name));

        _fields.Add(new BitField(name, _currentBitOffset, bitWidth, minValue, maxValue));
        _currentBitOffset += bitWidth;
        return this;
    }

    /// <summary>
    /// Set a field value
    /// </summary>
    public BitPacker<T> SetValue(string fieldName, BigInteger value)
    {
        var field = GetField(fieldName);
        field.ValidateValue(value);
        _values[fieldName] = value;
        return this;
    }

    /// <summary>
    /// Set a field value (convenience overload for int)
    /// </summary>
    public BitPacker<T> SetValue(string fieldName, int value) => SetValue(fieldName, new BigInteger(value));

    /// <summary>
    /// Set a field value (convenience overload for long)
    /// </summary>
    public BitPacker<T> SetValue(string fieldName, long value) => SetValue(fieldName, new BigInteger(value));

    /// <summary>
    /// Get a field value
    /// </summary>
    public BigInteger GetValue(string fieldName)
    {
        var field = GetField(fieldName);
        return _values.TryGetValue(fieldName, out var value) ? value : field.MinValue;
    }

    /// <summary>
    /// Get a field value as int
    /// </summary>
    public int GetValueAsInt(string fieldName) => (int)GetValue(fieldName);

    /// <summary>
    /// Get a field value as long
    /// </summary>
    public long GetValueAsLong(string fieldName) => (long)GetValue(fieldName);

    /// <summary>
    /// Pack all field values into the target type
    /// </summary>
    public T Pack()
    {
        BigInteger result = BigInteger.Zero;

        foreach (var field in Fields)
        {
            var value = GetValue(field.Name);
            var normalizedValue = field.NormalizeValue(value);
            result |= (normalizedValue & field.Mask) << field.BitOffset;
        }

        return ConvertFromBigInteger(result);
    }

    /// <summary>
    /// Unpack a value and populate field values
    /// </summary>
    public BitPacker<T> Unpack(T packedValue)
    {
        var bigIntValue = ConvertToBigInteger(packedValue);
        _values.Clear();

        foreach (var field in Fields)
        {
            var packedFieldValue = (bigIntValue >> field.BitOffset) & field.Mask;
            var originalValue = field.DenormalizeValue(packedFieldValue);
            _values[field.Name] = originalValue;
        }

        return this;
    }

    /// <summary>
    /// Create a new packer with the same field definition
    /// </summary>
    public BitPacker<T> CreateSimilar()
    {
        var newPacker = new BitPacker<T>();
        foreach (var field in Fields)
        {
            newPacker.AddFieldInternal(field.Name, field.BitWidth, field.MinValue, field.MaxValue);
        }
        return newPacker;
    }

    /// <summary>
    /// Get field information
    /// </summary>
    public BitField GetFieldInfo(string fieldName) => GetField(fieldName);

    /// <summary>
    /// Get all field definitions
    /// </summary>
    public IReadOnlyList<BitField> Fields => _fields.AsReadOnly();

    /// <summary>
    /// Get total bit width used
    /// </summary>
    public int TotalBitWidth => _currentBitOffset;

    /// <summary>
    /// Get total bytes needed (rounded up)
    /// </summary>
    public int TotalBytesNeeded => BitHelper.BytesForBits(TotalBitWidth);

    /// <summary>
    /// Bit Limit for type
    /// </summary>
    public int BitLimit()
    {
        return typeof(T) switch
        {
            var t when t == typeof(BigInteger) => int.MaxValue, // BigInteger has no limit
            var t when t == typeof(ulong) => 64,
            var t when t == typeof(long) => 63, // One bit for sign
            var t when t == typeof(uint) => 32,
            var t when t == typeof(int) => 31, // One bit for sign
            _ => 0
        };
    }

    private BitField GetField(string name)
    {
        var field = Fields.FirstOrDefault(f => f.Name == name);
        if (field == null)
            throw new ArgumentException($"Field '{name}' not found", nameof(name));
        return field;
    }

    private BigInteger ConvertToBigInteger(T value)
    {
        return value switch
        {
            BigInteger bi => bi,
            ulong ul => new BigInteger(ul),
            long l => new BigInteger(l),
            uint ui => new BigInteger(ui),
            int i => new BigInteger(i),
            _ => throw new NotSupportedException($"Type {typeof(T)} is not supported")
        };
    }

    private T ConvertFromBigInteger(BigInteger value)
    {
        if (typeof(T) == typeof(BigInteger))
            return (T)(object)value;
        if (typeof(T) == typeof(ulong))
            return (T)(object)(ulong)value;
        if (typeof(T) == typeof(long))
            return (T)(object)(long)value;
        if (typeof(T) == typeof(uint))
            return (T)(object)(uint)value;
        if (typeof(T) == typeof(int))
            return (T)(object)(int)value;

        throw new NotSupportedException($"Type {typeof(T)} is not supported");
    }
}
