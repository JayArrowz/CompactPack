using CompactPack.Packers;

namespace CompactPack.Test;

public class Bit32PackerTests
{
    [Fact]
    public void AddField_ExceedsLimit_ShouldThrow()
    {
        var packer = Bit32Packer.Create()
            .AddField("field1", 20)
            .AddField("field2", 10);

        Assert.Throws<InvalidOperationException>(() => packer.AddField("field3", 5));
    }

    [Fact]
    public void PackUnpack_RGBAColor_ShouldRoundTrip()
    {
        var packer = Bit32Packer.Create()
            .AddField("Red", PackRange.Of(255))
            .AddField("Green", PackRange.Of(255))
            .AddField("Blue", PackRange.Of(255))
            .AddField("Alpha", PackRange.Of(255));

        packer.SetValue("Red", 255)
              .SetValue("Green", 128)
              .SetValue("Blue", 64)
              .SetValue("Alpha", 200);

        var packed = packer.Pack();
        var unpacker = packer.CreateSimilar().Unpack(packed);

        Assert.Equal(255, unpacker.GetValueAsInt("Red"));
        Assert.Equal(128, unpacker.GetValueAsInt("Green"));
        Assert.Equal(64, unpacker.GetValueAsInt("Blue"));
        Assert.Equal(200, unpacker.GetValueAsInt("Alpha"));
    }
}
