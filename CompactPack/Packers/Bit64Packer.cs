namespace CompactPack.Packers;

/// <summary>
/// Specialized bit packer for 64-bit integers with automatic overflow checking
/// </summary>
public class Bit64Packer : VariableBitPacker<ulong, Bit64Packer>
{
    public static Bit64Packer Create() => new();
}
