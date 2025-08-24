using CompactPack.Packers;

namespace CompactPack.Test;

public class Bit64PackerTests
{
    [Fact]
    public void AddField_ExceedsLimit_ShouldThrow()
    {
        var packer = Bit64Packer.Create();

        Assert.Throws<InvalidOperationException>(() => packer.AddField("field1", 65));
    }

    [Fact]
    public void PackUnpack_LargeValues_ShouldRoundTrip()
    {
        var packer = Bit64Packer.Create()
            .AddField("timestamp", PackRange.Of((long)Math.Pow(2, 32) - 1))
            .AddField("userId", PackRange.Of((long)Math.Pow(2, 20) - 1))
            .AddField("flags", PackRange.Of(255));

        packer.SetValue("timestamp", 1609459200)
              .SetValue("userId", 12345)
              .SetValue("flags", 0b10110001);

        var packed = packer.Pack();
        var unpacker = packer.CreateSimilar().Unpack(packed);

        Assert.Equal(1609459200, unpacker.GetValueAsLong("timestamp"));
        Assert.Equal(12345, unpacker.GetValueAsLong("userId"));
        Assert.Equal(0b10110001, unpacker.GetValueAsInt("flags"));
    }
}
