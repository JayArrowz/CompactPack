using System.Numerics;

namespace CompactPack.Test;

public class BitPackerBigIntegerTests
{
    [Fact]
    public void AddField_ValidField_ShouldAddSuccessfully()
    {
        var packer = new BitPacker<BigInteger>();
        packer.AddField("test", 8);

        Assert.Equal(1, packer.FieldCount);
        Assert.Equal("test", packer.Fields[0].Name);
        Assert.Equal(8, packer.Fields[0].BitWidth);
    }

    [Fact]
    public void AddField_DuplicateName_ShouldThrow()
    {
        var packer = new BitPacker<BigInteger>();
        packer.AddField("test", 8);

        Assert.Throws<ArgumentException>(() => packer.AddField("test", 4));
    }

    [Fact]
    public void AddField_EmptyName_ShouldThrow()
    {
        var packer = new BitPacker<BigInteger>();
        Assert.Throws<ArgumentException>(() => packer.AddField("", 8));
    }

    [Fact]
    public void AddField_ZeroBits_ShouldThrow()
    {
        var packer = new BitPacker<BigInteger>();
        Assert.Throws<ArgumentOutOfRangeException>(() => packer.AddField("test", 0));
    }

    [Fact]
    public void SetValue_ValidValue_ShouldSetSuccessfully()
    {
        var packer = new BitPacker<BigInteger>()
            .AddField("test", 255);

        packer.SetValue("test", 100);
        Assert.Equal(100, packer.GetValue("test"));
    }

    [Fact]
    public void SetValue_InvalidValue_ShouldThrow()
    {
        var packer = new BitPacker<BigInteger>()
            .AddField("test", PackRange.Of(255));

        Assert.Throws<ArgumentOutOfRangeException>(() => packer.SetValue("test", 256));
    }

    [Fact]
    public void GetValue_NonexistentField_ShouldThrow()
    {
        var packer = new BitPacker<BigInteger>();
        Assert.Throws<ArgumentException>(() => packer.GetValue("nonexistent"));
    }

    [Fact]
    public void GetValue_UnsetField_ShouldReturnMinValue()
    {
        var packer = new BitPacker<BigInteger>()
            .AddField("test", PackRange.Of(10, 20));

        Assert.Equal(10, packer.GetValue("test")); // Should return minValue
    }

    [Fact]
    public void PackUnpack_SimpleFields_ShouldRoundTrip()
    {
        var packer = new BitPacker<BigInteger>()
            .AddField("health", 255)
            .AddField("mana", 255)
            .AddField("level", 100);

        packer.SetValue("health", 200)
              .SetValue("mana", 150)
              .SetValue("level", 42);

        var packed = packer.Pack();

        var unpacker = packer.CreateSimilar().Unpack(packed);

        Assert.Equal(200, unpacker.GetValue("health"));
        Assert.Equal(150, unpacker.GetValue("mana"));
        Assert.Equal(42, unpacker.GetValue("level"));
    }

    [Fact]
    public void PackUnpack_RangeFields_ShouldRoundTrip()
    {
        var packer = new BitPacker<BigInteger>()
            .AddField("temperature", PackRange.Of(-40, 85))
            .AddField("pressure", PackRange.Of(300, 1100));

        packer.SetValue("temperature", 23)
              .SetValue("pressure", 1013);

        var packed = packer.Pack();
        var unpacker = packer.CreateSimilar().Unpack(packed);

        Assert.Equal(23, unpacker.GetValue("temperature"));
        Assert.Equal(1013, unpacker.GetValue("pressure"));
    }

    [Fact]
    public void AddFields_MultipleSameBitWidth_ShouldAddAll()
    {
        var packer = new BitPacker<BigInteger>()
            .AddFields(6, "P1", "H1", "H2", "H3");

        Assert.Equal(4, packer.Fields.Count);
        Assert.All(packer.Fields, field => Assert.Equal(6, field.BitWidth));
    }

    [Fact]
    public void TotalBitWidth_ShouldCalculateCorrectly()
    {
        var packer = new BitPacker<BigInteger>()
            .AddField("field1", 8)
            .AddField("field2", 6)
            .AddField("field3", 10);

        Assert.Equal(24, packer.TotalBitWidth);
    }
}
