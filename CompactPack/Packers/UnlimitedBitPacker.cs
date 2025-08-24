using System.Numerics;

namespace CompactPack.Packers;

/// <summary>
/// Specialized bit packer for BigInteger (unlimited size)
/// </summary>
public class UnlimitedBitPacker : BitPacker<BigInteger>
{
    public static UnlimitedBitPacker Create() => new();
}