namespace CompactPack.Packers;

/// <summary>
/// Specialized bit packer for 64-bit signed integers with automatic overflow checking
/// </summary>
public class Bit64SignedPacker : VariableBitPacker<long, Bit64SignedPacker>
{
    public static Bit64SignedPacker Create() => new();
}
