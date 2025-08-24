namespace CompactPack.Packers;

using System;
using System.Numerics;

/// <summary>
/// Specialized bit packer for 256-bit integers (using BigInteger storage with 256-bit limit)
/// Perfect for blockchain addresses, large hash values, or crypto operations
/// </summary>
public class Bit256Packer : VariableBitPacker<BigInteger, Bit256Packer>
{
    public Bit256Packer() : base(MaxBitWidth)
    {
    }

    public const int MaxBitWidth = 256;

    public static Bit256Packer Create() => new();

    /// <summary>
    /// Get remaining bit capacity
    /// </summary>
    public int RemainingBits => MaxBitWidth - TotalBitWidth;

    /// <summary>
    /// Check if we can fit another field with specified bit width
    /// </summary>
    public bool CanFitField(int bitWidth) => RemainingBits >= bitWidth;

    /// <summary>
    /// Check if we can fit a field for the specified max value
    /// </summary>
    public bool CanFitField(BigInteger maxValue) => CanFitField(BitHelper.BitsForValue(maxValue));

    /// <summary>
    /// Get the maximum value that can be stored in remaining bits
    /// </summary>
    public BigInteger MaxValueForRemainingBits => RemainingBits > 0 ? (BigInteger.One << RemainingBits) - 1 : 0;

    /// <summary>
    /// Validate that the packed result fits within 256 bits
    /// </summary>
    public override BigInteger Pack()
    {
        int totalBytes = TotalBytesNeeded;
        byte[] buffer = new byte[totalBytes];

        for (int i = 0; i < FieldCount; i++)
        {
            var field = _fields[i];
            var normalizedValue = _values[i] - field.MinValue;
            WriteBitsToBuffer(buffer, normalizedValue, field.BitOffset, field.BitWidth);
        }

        return new BigInteger(buffer);
    }

    private void WriteBitsToBuffer(byte[] buffer, BigInteger value, int bitOffset, int bitWidth)
    {
        byte[] valueBytes = value.ToByteArray();
        if (bitOffset % 8 == 0 && bitWidth % 8 == 0)
        {
            int byteOffset = bitOffset / 8;
            int byteWidth = bitWidth / 8;

            int bytesToCopy = Math.Min(byteWidth, valueBytes.Length);
            Array.Copy(valueBytes, 0, buffer, byteOffset, bytesToCopy);

            if (bytesToCopy < byteWidth)
            {
                Array.Clear(buffer, byteOffset + bytesToCopy, byteWidth - bytesToCopy);
            }

            return;
        }

        for (int i = 0; i < bitWidth; i++)
        {
            int absoluteBitPos = bitOffset + i;
            int byteIndex = absoluteBitPos / 8;
            int bitIndex = absoluteBitPos % 8;

            bool bitSet;
            if (i < valueBytes.Length * 8)
            {
                int valueByteIndex = i / 8;
                int valueBitIndex = i % 8;
                bitSet = (valueBytes[valueByteIndex] & (1 << valueBitIndex)) != 0;
            }
            else
            {
                bitSet = false;
            }

            if (bitSet)
            {
                buffer[byteIndex] |= (byte)(1 << bitIndex);
            }
            else
            {
                buffer[byteIndex] &= (byte)~(1 << bitIndex);
            }
        }
    }

    /// <summary>
    /// Create a packer optimized for Ethereum-style addresses and values
    /// </summary>
    public static Bit256Packer CreateEthereumStyle()
    {
        return Create()
            .AddField("Address", 160)           // Ethereum address (20 bytes = 160 bits)
            .AddField("Value", 64)              // Value in wei (up to ~18 ETH with good precision)
            .AddField("Nonce", 32);             // Transaction nonce (up to 4B transactions)
    }

    /// <summary>
    /// Create a packer for hash-based operations
    /// </summary>
    public static Bit256Packer CreateHashStyle()
    {
        return Create()
            .AddField("Hash", 256);             // Full 256-bit hash (SHA-256, Keccak-256, etc.)
    }

    /// <summary>
    /// Create a packer with multiple 32-bit fields (common for IDs, timestamps, etc.)
    /// </summary>
    public static Bit256Packer Create8x32Bit()
    {
        return Create()
            .AddFields(32, "Field0", "Field1", "Field2", "Field3",
                           "Field4", "Field5", "Field6", "Field7");
    }
}