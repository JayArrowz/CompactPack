using System.Numerics;

namespace CompactPack.Packers;

/// <summary>
/// Specialized bit packer for BigInteger (unlimited size)
/// </summary>
public class UnlimitedBitPacker : VariableBitPacker<BigInteger, UnlimitedBitPacker>
{
    public UnlimitedBitPacker() : base(int.MaxValue) { }

    public static UnlimitedBitPacker Create() => new();
}