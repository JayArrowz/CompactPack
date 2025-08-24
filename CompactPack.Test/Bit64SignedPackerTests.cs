using CompactPack.Packers;

namespace CompactPack.Test;

public class Bit64SignedPackerTests
{
    [Fact]
    public void Create_ShouldReturnNewInstance()
    {
        var packer = Bit64SignedPacker.Create();
        Assert.NotNull(packer);
        Assert.IsType<Bit64SignedPacker>(packer);
    }

    [Fact]
    public void BitLimit_ShouldBe63()
    {
        var packer = Bit64SignedPacker.Create();
        Assert.Equal(63, packer.CurrentBitLimit);
    }

    [Fact]
    public void AddField_WithinLimit_ShouldAddSuccessfully()
    {
        var packer = Bit64SignedPacker.Create()
            .AddField("field1", 20)
            .AddField("field2", 20)
            .AddField("field3", 20)
            .AddField("field4", 3); // Total: 63 bits (exactly at limit)

        Assert.Equal(4, packer.Fields.Count);
        Assert.Equal(63, packer.TotalBitWidth);
    }

    [Fact]
    public void AddField_ExceedsLimit_ShouldThrow()
    {
        var packer = Bit64SignedPacker.Create()
            .AddField("field1", 32)
            .AddField("field2", 30);

        // Adding 2 more bits would exceed 63-bit limit
        Assert.Throws<InvalidOperationException>(() => packer.AddField("field3", 2));
    }

    [Fact]
    public void PackUnpack_LargeValues_ShouldRoundTrip()
    {
        var packer = Bit64SignedPacker.Create()
            .AddField("largePositive", PackRange.Of(0, 1000000))    // 20 bits
            .AddField("largeNegative", PackRange.Of(-1000000, 0))   // 20 bits
            .AddField("mediumValue", PackRange.Of(0, 8388607));     // 23 bits (total: 63 bits)

        packer.SetValue("largePositive", 500000)
              .SetValue("largeNegative", -250000)
              .SetValue("mediumValue", 4000000);

        var packed = packer.Pack();
        var unpacker = packer.CreateSimilar().Unpack(packed);

        Assert.Equal(500000, unpacker.GetValueAsLong("largePositive"));
        Assert.Equal(-250000, unpacker.GetValueAsLong("largeNegative"));
        Assert.Equal(4000000, unpacker.GetValueAsLong("mediumValue"));
    }


    [Fact]
    public void PackUnpack_ExtremeLongValues_ShouldRoundTrip()
    {
        var packer = Bit64SignedPacker.Create()
            .AddField("veryLarge", PackRange.Of(0, 1099511627775L)); // 40 bits

        var testValue = 549755813887L; // Large but within range
        packer.SetValue("veryLarge", testValue);

        var packed = packer.Pack();
        var unpacker = packer.CreateSimilar().Unpack(packed);

        Assert.Equal(testValue, unpacker.GetValueAsLong("veryLarge"));
    }

    [Fact]
    public void PackUnpack_NegativeLongRange_ShouldRoundTrip()
    {
        var packer = Bit64SignedPacker.Create()
            .AddField("bigRange", PackRange.Of(-2147483648L, 2147483647L)); // 32-bit signed range

        packer.SetValue("bigRange", -1000000000);

        var packed = packer.Pack();
        var unpacker = packer.CreateSimilar().Unpack(packed);

        Assert.Equal(-1000000000, unpacker.GetValueAsLong("bigRange"));
    }

    [Fact]
    public void AddFieldWithBytes_ShouldCheckLimits()
    {
        var packer = Bit64SignedPacker.Create()
            .AddField("field1", 40);

        // Adding 4 bytes (32 bits) would exceed limit (40 + 32 = 72 > 63)
        Assert.Throws<InvalidOperationException>(() =>
            packer.AddFieldWithBytes("field2", 4));
    }

    [Fact]
    public void AddFieldWithBytes_WithinLimit_ShouldWork()
    {
        var packer = Bit64SignedPacker.Create()
            .AddField("field1", 32)
            .AddFieldWithBytes("field2", 3); // 24 bits, total = 56 bits

        Assert.Equal(2, packer.Fields.Count);
        Assert.Equal(56, packer.TotalBitWidth);
    }

    [Fact]
    public void FluentAPI_ComplexChaining_ShouldWork()
    {
        var packer = Bit64SignedPacker.Create()
            .AddField("userId", PackRange.Of(1048575))    // 20 bits
            .AddField("sessionId", PackRange.Of(65535))   // 16 bits  
            .AddField("flags", PackRange.Of(8388607));    // 23 bits (total: 59 bits)

        var packed = packer.SetValue("userId", 12345)
                           .SetValue("sessionId", 9876)
                           .SetValue("flags", 4000000)
                           .Pack();

        Assert.IsType<long>(packed);
        Assert.NotEqual(0L, packed);
    }

    [Fact]
    public void ErrorMessage_ShouldBeDescriptive()
    {
        var packer = Bit64SignedPacker.Create()
            .AddField("field1", 50);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            packer.AddField("field2", 20)); // 50 + 20 = 70 > 63

        Assert.Contains("field2", exception.Message);
        Assert.Contains("63-bit limit", exception.Message);
        Assert.Contains("50", exception.Message); // Current usage
    }
}
