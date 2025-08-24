using CompactPack.Packers;
using System.Numerics;

namespace CompactPack.Test;

public class UnlimitedBitPackerTests
{
    [Fact]
    public void PackUnpack_VeryLargeValues_ShouldRoundTrip()
    {
        var packer = UnlimitedBitPacker.Create()
            .AddField("huge1", PackRange.Of(BigInteger.Parse("123456789012345678901234567890")))
            .AddField("huge2", PackRange.Of(BigInteger.Parse("999888777666555444333222111001")));

        var value1 = BigInteger.Parse("111222333444555666777888999000");
        var value2 = BigInteger.Parse("999888777666555444333222111000");

        packer.SetValue("huge1", value1)
              .SetValue("huge2", value2);

        var packed = packer.Pack();
        var unpacker = packer.CreateSimilar().Unpack(packed);

        Assert.Equal(value1, unpacker.GetValue("huge1"));
        Assert.Equal(value2, unpacker.GetValue("huge2"));
    }


    [Fact]
    public void TotalBytesNeeded_ShouldCalculateCorrectly()
    {
        var packer = UnlimitedBitPacker.Create()
            .AddField("field1", PackRange.Of(10))  // 4 bits
            .AddField("field2", PackRange.Of(100)) // 7 bits
            .AddField("field3", PackRange.Of(1000)); // 10 bits
        var expectedBits = BitHelper.BitsForValue(10) + BitHelper.BitsForValue(100) + BitHelper.BitsForValue(1000);
        var expectedBytes = BitHelper.BytesForBits(expectedBits);


        // Total: 4 + 7 + 10 = 21 bits = 3 bytes (rounded up)
        Assert.Equal(3, packer.TotalBytesNeeded);
    }
}
