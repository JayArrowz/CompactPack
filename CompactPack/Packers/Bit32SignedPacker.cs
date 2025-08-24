namespace CompactPack.Packers;

/// <summary>
/// Specialized bit packer for 32-bit signed integers with automatic overflow checking
/// </summary>
public class Bit32SignedPacker : VariableBitPacker<int, Bit32SignedPacker>
{
    public static Bit32SignedPacker Create() => new();
}
