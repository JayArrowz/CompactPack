namespace CompactPack.Packers;

/// <summary>
/// Specialized bit packer for 32-bit integers with automatic overflow checking
/// </summary>
public class Bit32Packer : VariableBitPacker<uint, Bit32Packer>
{
    public static Bit32Packer Create() => new();
}