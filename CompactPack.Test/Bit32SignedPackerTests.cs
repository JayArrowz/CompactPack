using CompactPack.Packers;

namespace CompactPack.Test;

public class Bit32SignedPackerTests
{
    [Fact]
    public void Create_ShouldReturnNewInstance()
    {
        var packer = Bit32SignedPacker.Create();
        Assert.NotNull(packer);
        Assert.IsType<Bit32SignedPacker>(packer);
    }

    [Fact]
    public void BitLimit_ShouldBe31()
    {
        var packer = Bit32SignedPacker.Create();
        Assert.Equal(31, packer.CurrentBitLimit);
    }

    [Fact]
    public void AddField_WithinLimit_ShouldAddSuccessfully()
    {
        var packer = Bit32SignedPacker.Create()
            .AddField("field1", 10)
            .AddField("field2", 15)
            .AddField("field3", 6); // Total: 31 bits (exactly at limit)

        Assert.Equal(3, packer.Fields.Count);
        Assert.Equal(31, packer.TotalBitWidth);
    }

    [Fact]
    public void AddField_ExceedsLimit_ShouldThrow()
    {
        var packer = Bit32SignedPacker.Create()
            .AddField("field1", 20)
            .AddField("field2", 10);

        // Adding 2 more bits would exceed 31-bit limit
        Assert.Throws<InvalidOperationException>(() => packer.AddField("field3", 2));
    }

    [Fact]
    public void AddField_WithRange_ShouldCalculateCorrectBits()
    {
        var packer = Bit32SignedPacker.Create()
            .AddField("small", PackRange.Of(15))      // 4 bits
            .AddField("medium", PackRange.Of(255))    // 8 bits
            .AddField("large", PackRange.Of(65535));  // 16 bits

        Assert.Equal(28, packer.TotalBitWidth); // 4 + 8 + 16 = 28 bits
    }

    [Fact]
    public void AddField_RangeExceedsLimit_ShouldThrow()
    {
        var packer = Bit32SignedPacker.Create()
            .AddField("field1", 20);

        // Adding a range that needs 12 more bits would exceed limit (20 + 12 = 32 > 31)
        Assert.Throws<InvalidOperationException>(() =>
            packer.AddField("field2", PackRange.Of(4095))); // 4095 needs 12 bits
    }

    [Fact]
    public void PackUnpack_PositiveValues_ShouldRoundTrip()
    {
        var packer = Bit32SignedPacker.Create()
            .AddField("value1", PackRange.Of(100))
            .AddField("value2", PackRange.Of(1000))
            .AddField("value3", PackRange.Of(50));

        packer.SetValue("value1", 75)
              .SetValue("value2", 500)
              .SetValue("value3", 25);

        var packed = packer.Pack();
        var unpacker = packer.CreateSimilar().Unpack(packed);

        Assert.Equal(75, unpacker.GetValueAsInt("value1"));
        Assert.Equal(500, unpacker.GetValueAsInt("value2"));
        Assert.Equal(25, unpacker.GetValueAsInt("value3"));
    }

    [Fact]
    public void PackUnpack_NegativeValues_ShouldRoundTrip()
    {
        var packer = Bit32SignedPacker.Create()
            .AddField("temperature", PackRange.Of(-50, 50))
            .AddField("offset", PackRange.Of(-100, 100));

        packer.SetValue("temperature", -25)
              .SetValue("offset", -75);

        var packed = packer.Pack();
        var unpacker = packer.CreateSimilar().Unpack(packed);

        Assert.Equal(-25, unpacker.GetValueAsInt("temperature"));
        Assert.Equal(-75, unpacker.GetValueAsInt("offset"));
    }

    [Fact]
    public void PackUnpack_MixedValues_ShouldRoundTrip()
    {
        var packer = Bit32SignedPacker.Create()
            .AddField("positive", PackRange.Of(0, 1000))
            .AddField("negative", PackRange.Of(-500, 0))
            .AddField("mixed", PackRange.Of(-100, 100));

        packer.SetValue("positive", 750)
              .SetValue("negative", -250)
              .SetValue("mixed", 50);

        var packed = packer.Pack();
        var unpacker = packer.CreateSimilar().Unpack(packed);

        Assert.Equal(750, unpacker.GetValueAsInt("positive"));
        Assert.Equal(-250, unpacker.GetValueAsInt("negative"));
        Assert.Equal(50, unpacker.GetValueAsInt("mixed"));
    }

    [Fact]
    public void AddFields_MultipleSameBitWidth_ShouldWork()
    {
        var packer = Bit32SignedPacker.Create()
            .AddFields(5, "field1", "field2", "field3", "field4", "field5"); // 25 bits total

        Assert.Equal(5, packer.Fields.Count);
        Assert.Equal(25, packer.TotalBitWidth);
    }

    [Fact]
    public void AddFields_MultipleSameRange_ShouldWork()
    {
        var packer = Bit32SignedPacker.Create()
            .AddFields(PackRange.Of(31), "f1", "f2", "f3", "f4", "f5"); // 5 bits each, 25 total

        Assert.Equal(5, packer.Fields.Count);
        Assert.Equal(25, packer.TotalBitWidth);
    }

    [Fact]
    public void CreateSimilar_ShouldCopyFieldsButNotValues()
    {
        var originalPacker = Bit32SignedPacker.Create()
            .AddField("field1", PackRange.Of(100))
            .AddField("field2", PackRange.Of(200));

        originalPacker.SetValue("field1", 50)
                     .SetValue("field2", 150);

        var similarPacker = originalPacker.CreateSimilar();

        Assert.Equal(originalPacker.Fields.Count, similarPacker.Fields.Count);
        Assert.Equal(0, similarPacker.GetValue("field1")); // Should be min value (0)
        Assert.Equal(0, similarPacker.GetValue("field2")); // Should be min value (0)
    }

    [Fact]
    public void FluentAPI_ShouldChainCorrectly()
    {
        var result = Bit32SignedPacker.Create()
            .AddField("health", PackRange.Of(1000))
            .AddField("mana", PackRange.Of(500))
            .SetValue("health", 750)
            .SetValue("mana", 250)
            .Pack();

        Assert.IsType<int>(result);
        Assert.NotEqual(0, result);
    }
}
