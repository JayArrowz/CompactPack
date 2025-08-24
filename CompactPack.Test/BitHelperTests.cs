namespace CompactPack.Test;

public class BitHelperTests
{
    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 1)]
    [InlineData(2, 2)]
    [InlineData(3, 2)]
    [InlineData(4, 3)]
    [InlineData(7, 3)]
    [InlineData(8, 4)]
    [InlineData(15, 4)]
    [InlineData(255, 8)]
    [InlineData(256, 9)]
    public void BitsForValue_ShouldReturnCorrectBits(int maxValue, int expectedBits)
    {
        Assert.Equal(expectedBits, BitHelper.BitsForValue(maxValue));
    }

    [Theory]
    [InlineData(0, 0, 1)]
    [InlineData(0, 1, 1)]
    [InlineData(0, 3, 2)]
    [InlineData(1, 8, 3)]
    [InlineData(100, 200, 7)]
    [InlineData(-32, 31, 6)]
    public void BitsForRange_ShouldReturnCorrectBits(int min, int max, int expectedBits)
    {
        Assert.Equal(expectedBits, BitHelper.BitsForRange(min, max));
    }

    [Fact]
    public void BitsForRange_InvalidRange_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => BitHelper.BitsForRange(10, 5));
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(8, 1)]
    [InlineData(9, 2)]
    [InlineData(32, 4)]
    [InlineData(33, 5)]
    public void BytesForBits_ShouldRoundUpCorrectly(int bits, int expectedBytes)
    {
        Assert.Equal(expectedBytes, BitHelper.BytesForBits(bits));
    }
}

public class BitFieldTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(32)]
    [InlineData(63)]
    public void ValidateValue_ValidValues_ShouldNotThrow(int value)
    {
        var field = new BitField("test", 0, 6, 0, 63);
        var exception = Record.Exception(() => field.ValidateValue(value));
        Assert.Null(exception);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(64)]
    public void ValidateValue_InvalidValues_ShouldThrow(int value)
    {
        var field = new BitField("test", 0, 6, 0, 63);
        Assert.Throws<ArgumentOutOfRangeException>(() => field.ValidateValue(value));
    }

    [Theory]
    [InlineData(100, 0)]
    [InlineData(150, 50)]
    [InlineData(200, 100)]
    public void NormalizeValue_ShouldShiftToZeroBased(int input, int expected)
    {
        var field = new BitField("test", 0, 7, 100, 200);
        Assert.Equal(expected, field.NormalizeValue(input));
    }

    [Theory]
    [InlineData(0, 100)]
    [InlineData(50, 150)]
    [InlineData(100, 200)]
    public void DenormalizeValue_ShouldShiftBackToOriginalRange(int input, int expected)
    {
        var field = new BitField("test", 0, 7, 100, 200);
        Assert.Equal(expected, field.DenormalizeValue(input));
    }

    [Fact]
    public void Mask_ShouldReturnCorrectBitMask()
    {
        var field = new BitField("test", 0, 6, 0, 63);
        Assert.Equal(63, field.Mask); // 2^6 - 1 = 63 = 0x3F
    }
}
