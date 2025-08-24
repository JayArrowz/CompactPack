using CompactPack.Packers;
using System.Numerics;

namespace CompactPack.Test;

public class Bit256PackerTests
{
    [Fact]
    public void AddField_ExceedsLimit_ShouldThrow()
    {
        var packer = Bit256Packer.Create();

        Assert.Throws<InvalidOperationException>(() => packer.AddField("field1", 257));
    }

    [Fact]
    public void RemainingBits_ShouldCalculateCorrectly()
    {
        var packer = Bit256Packer.Create()
            .AddField("field1", PackRange.Of(100))
            .AddField("field2", PackRange.Of(50));

        var expectedRemaining = 256 - BitHelper.BitsForValue(100) - BitHelper.BitsForValue(50);
        Assert.Equal(expectedRemaining, packer.RemainingBits);
    }

    [Fact]
    public void CanFitField_ShouldReturnCorrectly()
    {
        var packer = Bit256Packer.Create()
            .AddField("field1", PackRange.Of(200)); // Uses 8 bits

        Assert.True(packer.CanFitField(7)); // Should fit
        Assert.True(packer.CanFitField(248)); // Should fit exactly
        Assert.False(packer.CanFitField(249)); // Should not fit
    }

    [Fact]
    public void CreateEthereumStyle_ShouldHaveCorrectFields()
    {
        var packer = Bit256Packer.CreateEthereumStyle();

        Assert.Equal(3, packer.FieldCount);
        Assert.Contains(packer.Fields, f => f.Name == "Address");
        Assert.Contains(packer.Fields, f => f.Name == "Value");
        Assert.Contains(packer.Fields, f => f.Name == "Nonce");
        Assert.True(packer.TotalBitWidth <= 256);
    }

    [Fact]
    public void CreateHashStyle_ShouldUseAll256Bits()
    {
        var packer = Bit256Packer.CreateHashStyle();

        Assert.Equal(packer.FieldCount, 1);
        Assert.Equal("Hash", packer.Fields[0].Name);
        Assert.Equal(256, packer.TotalBitWidth);
    }

    [Fact]
    public void Create8x32Bit_ShouldUseAll256Bits()
    {
        var packer = Bit256Packer.Create8x32Bit();

        Assert.Equal(8, packer.Fields.Count);
        Assert.Equal(256, packer.TotalBitWidth);
    }

    [Fact]
    public void PackUnpack_DNAStyle_ShouldRoundTrip()
    {
        var packer = Bit256Packer.Create();

        // Add 10 parts with 4 fields each (6 bits per field)
        for (int i = 0; i < 10; i++)
        {
            packer.AddField($"Part{i}_P1", PackRange.Of(63))
                  .AddField($"Part{i}_H1", PackRange.Of(63))
                  .AddField($"Part{i}_H2", PackRange.Of(63))
                  .AddField($"Part{i}_H3", PackRange.Of(63));
        }

        // Set values
        for (int i = 0; i < 10; i++)
        {
            packer.SetValue($"Part{i}_P1", i * 4)
                  .SetValue($"Part{i}_H1", i * 4 + 1)
                  .SetValue($"Part{i}_H2", i * 4 + 2)
                  .SetValue($"Part{i}_H3", i * 4 + 3);
        }

        var packed = packer.Pack();
        var unpacker = packer.CreateSimilar().Unpack(packed);

        // Verify values
        for (int i = 0; i < 10; i++)
        {
            Assert.Equal(i * 4, unpacker.GetValueAsInt($"Part{i}_P1"));
            Assert.Equal(i * 4 + 1, unpacker.GetValueAsInt($"Part{i}_H1"));
            Assert.Equal(i * 4 + 2, unpacker.GetValueAsInt($"Part{i}_H2"));
            Assert.Equal(i * 4 + 3, unpacker.GetValueAsInt($"Part{i}_H3"));
        }
    }

    [Fact]
    public void MaxValueForRemainingBits_ShouldCalculateCorrectly()
    {
        var packer = Bit256Packer.Create()
            .AddField("field1", PackRange.Of(15)); // Uses 4 bits

        var expectedMaxValue = (BigInteger.One << (256 - 4)) - 1;
        Assert.Equal(expectedMaxValue, packer.MaxValueForRemainingBits);
    }
}
