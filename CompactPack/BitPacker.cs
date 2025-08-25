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
    internal BitField[] _fields = Array.Empty<BitField>();
    internal BigInteger[] _values = Array.Empty<BigInteger>();
    private Dictionary<string, int> _fieldIndexMap = new();
    private int _fieldCount = 0;
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
        if (_fieldIndexMap.ContainsKey(name))
            throw new ArgumentException($"Field '{name}' already exists", nameof(name));

        BigInteger normalizedMax = maxValue - minValue;

        if (typeof(T) == typeof(uint))
        {
            if (normalizedMax > uint.MaxValue)
            {
                throw new ArgumentException($"Field '{name}' normalized range [0, {normalizedMax}] doesn't fit in uint range [0, {uint.MaxValue}]");
            }
        }
        else if (typeof(T) == typeof(int))
        {
            if (normalizedMax > int.MaxValue)
            {
                throw new ArgumentException($"Field '{name}' normalized range [0, {normalizedMax}] doesn't fit in int range [0, {int.MaxValue}]");
            }
        }
        else if (typeof(T) == typeof(ulong))
        {
            if (normalizedMax > ulong.MaxValue)
            {
                throw new ArgumentException($"Field '{name}' normalized range [0, {normalizedMax}] doesn't fit in ulong range [0, {ulong.MaxValue}]");
            }
        }
        else if (typeof(T) == typeof(long))
        {
            if (normalizedMax > long.MaxValue)
            {
                throw new ArgumentException($"Field '{name}' normalized range [0, {normalizedMax}] doesn't fit in long range [0, {long.MaxValue}]");
            }
        }

        if (_fieldCount >= _fields.Length)
        {
            int newSize = Math.Max(4, _fields.Length * 2);
            Array.Resize(ref _fields, newSize);
            Array.Resize(ref _values, newSize);
        }

        _fields[_fieldCount] = new BitField(name, _currentBitOffset, bitWidth, minValue, maxValue);
        _values[_fieldCount] = minValue;
        _fieldIndexMap[name] = _fieldCount;
        _fieldCount++;
        _currentBitOffset += bitWidth;
        return this;
    }

    /// <summary>
    /// Gets field index
    /// </summary>
    /// <param name="fieldName">The field name</param>
    /// <returns>Index of the field</returns>
    /// <exception cref="ArgumentException"></exception>
    public int GetFieldIndex(string fieldName)
    {
        if (!_fieldIndexMap.TryGetValue(fieldName, out int index))
            throw new ArgumentException($"Field '{fieldName}' not found", nameof(fieldName));

        return index;
    }

    /// <summary>
    /// Set a field value
    /// </summary>
    public BitPacker<T> SetValue(string fieldName, BigInteger value)
    {
        var index = GetFieldIndex(fieldName);
        var field = _fields[index];
        field.ValidateValue(value);
        _values[index] = value;
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
        var index = GetFieldIndex(fieldName);
        return _values[index];
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
    public virtual T Pack()
    {
        BigInteger result = BigInteger.Zero;

        for (int i = 0; i < _fieldCount; i++)
        {
            var field = _fields[i];
            var value = _values[i];
            var normalizedValue = value - field.MinValue;
            result |= (normalizedValue & field.Mask) << field.BitOffset;
        }

        return ConvertFromBigInteger(result);
    }

    /// <summary>
    /// Unpack a value and populate field values
    /// </summary>
    public virtual BitPacker<T> Unpack(T packedValue)
    {
        var bigIntValue = ConvertToBigInteger(packedValue);

        for (int i = 0; i < _fieldCount; i++)
        {
            var field = _fields[i];
            var packedFieldValue = (bigIntValue >> field.BitOffset);
            var mask = (BigInteger.One << field.BitWidth) - 1;
            packedFieldValue &= mask;
            _values[i] = packedFieldValue + field.MinValue;
        }

        return this;
    }

    /// <summary>
    /// Create a new packer with the same field definition
    /// </summary>
    public BitPacker<T> CreateSimilar()
    {
        var newPacker = new BitPacker<T>();
        for (int i = 0; i < _fieldCount; i++)
        {
            BitField field = Fields[i];
            newPacker.AddFieldInternal(field.Name, field.BitWidth, field.MinValue, field.MaxValue);
        }
        return newPacker;
    }

    /// <summary>
    /// Resets values to field.MinValue
    /// </summary>
    public BitPacker<T> ResetValues()
    {
        for (int i = 0; i < _fieldCount; i++)
        {
            _values[i] = _fields[i].MinValue;
        }
        return this;
    }

    /// <summary>
    /// Get field information
    /// </summary>
    public BitField GetFieldInfo(string fieldName) => GetField(fieldName);

    /// <summary>
    /// Get all field definitions
    /// </summary>
    public IReadOnlyList<BitField> Fields => _fields;

    /// <summary>
    /// Get all values
    /// </summary>
    public IReadOnlyList<BigInteger> Values => _values;

    /// <summary>
    /// Get total bit width used
    /// </summary>
    public int TotalBitWidth => _currentBitOffset;

    /// <summary>
    /// Get total bytes needed (rounded up)
    /// </summary>
    public int TotalBytesNeeded => BitHelper.BytesForBits(TotalBitWidth);

    /// <summary>
    /// The field count
    /// </summary>
    public int FieldCount => _fieldCount;

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
        var index = GetFieldIndex(name);
        return _fields[index];
    }

    private BigInteger ConvertToBigInteger(T value)
    {
        // Use pattern matching for better performance
        return value switch
        {
            BigInteger bi => bi,
            ulong ul => new BigInteger(ul),
            long l => new BigInteger(l),
            uint ui => new BigInteger(ui),
            int i => new BigInteger(i),
            ushort us => new BigInteger(us),
            short s => new BigInteger(s),
            byte b => new BigInteger(b),
            sbyte sb => new BigInteger(sb),
            _ => throw new NotSupportedException($"Type {typeof(T)} is not supported")
        };
    }

    private T ConvertFromBigInteger(BigInteger value)
    {
        if (typeof(T) == typeof(BigInteger))
        {
            return (T)(object)value;
        }
        else if (typeof(T) == typeof(ulong))
        {
            if (value < 0 || value > ulong.MaxValue)
                throw new OverflowException($"Value {value} is out of ulong range [0, {ulong.MaxValue}]");
            return (T)(object)(ulong)value;
        }
        else if (typeof(T) == typeof(long))
        {
            if (value < long.MinValue || value > long.MaxValue)
                throw new OverflowException($"Value {value} is out of long range [{long.MinValue}, {long.MaxValue}]");
            return (T)(object)(long)value;
        }
        else if (typeof(T) == typeof(uint))
        {
            if (value < 0 || value > uint.MaxValue)
                throw new OverflowException($"Value {value} is out of uint range [0, {uint.MaxValue}]");
            return (T)(object)(uint)value;
        }
        else if (typeof(T) == typeof(int))
        {
            if (value < int.MinValue || value > int.MaxValue)
                throw new OverflowException($"Value {value} is out of int range [{int.MinValue}, {int.MaxValue}]");
            return (T)(object)(int)value;
        }
        else if (typeof(T) == typeof(ushort))
        {
            if (value < 0 || value > ushort.MaxValue)
                throw new OverflowException($"Value {value} is out of ushort range [0, {ushort.MaxValue}]");
            return (T)(object)(ushort)value;
        }
        else if (typeof(T) == typeof(short))
        {
            if (value < short.MinValue || value > short.MaxValue)
                throw new OverflowException($"Value {value} is out of short range [{short.MinValue}, {short.MaxValue}]");
            return (T)(object)(short)value;
        }
        else if (typeof(T) == typeof(byte))
        {
            if (value < 0 || value > byte.MaxValue)
                throw new OverflowException($"Value {value} is out of byte range [0, {byte.MaxValue}]");
            return (T)(object)(byte)value;
        }
        else if (typeof(T) == typeof(sbyte))
        {
            if (value < sbyte.MinValue || value > sbyte.MaxValue)
                throw new OverflowException($"Value {value} is out of sbyte range [{sbyte.MinValue}, {sbyte.MaxValue}]");
            return (T)(object)(sbyte)value;
        }
        else
        {
            throw new NotSupportedException($"Type {typeof(T)} is not supported");
        }
    }

    public ref BigInteger GetValueRef(string fieldName)
    {
        if (!_fieldIndexMap.TryGetValue(fieldName, out int index))
            throw new ArgumentException($"Field '{fieldName}' not found", nameof(fieldName));

        return ref _values[index];
    }

    public ref BigInteger GetValueRef(int fieldIndex)
    {
        if (fieldIndex < 0 || fieldIndex >= _fieldCount)
            throw new ArgumentOutOfRangeException(nameof(fieldIndex));

        return ref _values[fieldIndex];
    }
}
