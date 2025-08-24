using System.Numerics;

namespace CompactPack.Test;

public class EdgeCaseTests
{
    [Fact]
    public void PackUnpack_ZeroValues_ShouldWork()
    {
        var packer = new BitPacker<BigInteger>()
            .AddField("zero1", 255)
            .AddField("zero2", 1000);

        // Don't set any values - they should default to min values (0)
        var packed = packer.Pack();
        var unpacker = packer.CreateSimilar().Unpack(packed);

        Assert.Equal(0, unpacker.GetValue("zero1"));
        Assert.Equal(0, unpacker.GetValue("zero2"));
    }

    [Fact]
    public void PackUnpack_MaxValues_ShouldWork()
    {
        var packer = new BitPacker<BigInteger>()
            .AddField("max1", 255)
            .AddField("max2", 1000);

        packer.SetValue("max1", 255)
              .SetValue("max2", 1000);

        var packed = packer.Pack();
        var unpacker = packer.CreateSimilar().Unpack(packed);

        Assert.Equal(255, unpacker.GetValue("max1"));
        Assert.Equal(1000, unpacker.GetValue("max2"));
    }

    [Fact]
    public void PackUnpack_SingleBitFields_ShouldWork()
    {
        var packer = new BitPacker<BigInteger>()
            .AddField("bit1", 1)
            .AddField("bit2", 1)
            .AddField("bit3", 1);

        packer.SetValue("bit1", 1)
              .SetValue("bit2", 0)
              .SetValue("bit3", 1);

        var packed = packer.Pack();
        var unpacker = packer.CreateSimilar().Unpack(packed);

        Assert.Equal(1, unpacker.GetValue("bit1"));
        Assert.Equal(0, unpacker.GetValue("bit2"));
        Assert.Equal(1, unpacker.GetValue("bit3"));
    }

    [Fact]
    public void CreateSimilar_ShouldCopyFieldsButNotValues()
    {
        var originalPacker = new BitPacker<BigInteger>()
            .AddField("field1", 255)
            .AddField("field2", 1000);

        originalPacker.SetValue("field1", 100)
                     .SetValue("field2", 500);

        var similarPacker = originalPacker.CreateSimilar();

        Assert.Equal(originalPacker.Fields.Count, similarPacker.Fields.Count);
        Assert.Equal(0, similarPacker.GetValue("field1")); // Should be min value, not 100
        Assert.Equal(0, similarPacker.GetValue("field2")); // Should be min value, not 500
    }
}

