namespace CompactPack.Test;

public class PackRangeTests
{
    [Theory]
    [InlineData(10, 0, 10)]
    [InlineData(100, 0, 100)]
    [InlineData(0, 0, 0)]
    public void Of_SingleValue_ShouldCreateZeroBasedRange(int maxValue, int expectedMin, int expectedMax)
    {
        var range = PackRange.Of(maxValue);
        Assert.Equal(expectedMin, range.Min);
        Assert.Equal(expectedMax, range.Max);
    }

    [Theory]
    [InlineData(5, 10, 5, 10)]
    [InlineData(-10, 10, -10, 10)]
    [InlineData(100, 200, 100, 200)]
    public void Of_MinMax_ShouldCreateCorrectRange(int min, int max, int expectedMin, int expectedMax)
    {
        var range = PackRange.Of(min, max);
        Assert.Equal(expectedMin, range.Min);
        Assert.Equal(expectedMax, range.Max);
    }

    [Fact]
    public void Of_InvalidRange_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => PackRange.Of(10, 5));
    }

    [Theory]
    [InlineData(0, 10, 10)]
    [InlineData(5, 10, 5)]
    [InlineData(-10, 10, 20)]
    public void Span_ShouldCalculateCorrectly(int min, int max, int expectedSpan)
    {
        var range = PackRange.Of(min, max);
        Assert.Equal(expectedSpan, range.Span);
    }

    [Theory]
    [InlineData(0, 10, 11)]
    [InlineData(5, 10, 6)]
    [InlineData(-10, 10, 21)]
    public void ValueCount_ShouldCalculateCorrectly(int min, int max, int expectedCount)
    {
        var range = PackRange.Of(min, max);
        Assert.Equal(expectedCount, range.ValueCount);
    }

    [Theory]
    [InlineData(0, 10, 4)]    // 0-10 needs 4 bits
    [InlineData(0, 100, 7)]   // 0-100 needs 7 bits
    [InlineData(0, 1000, 10)] // 0-1000 needs 10 bits
    [InlineData(5, 10, 3)]    // 5-10 needs 3 bits (6 values)
    public void BitsRequired_ShouldCalculateCorrectly(int min, int max, int expectedBits)
    {
        var range = PackRange.Of(min, max);
        Assert.Equal(expectedBits, range.BitsRequired);
    }

    [Theory]
    [InlineData(0, 10, 5, true)]
    [InlineData(0, 10, 0, true)]
    [InlineData(0, 10, 10, true)]
    [InlineData(0, 10, -1, false)]
    [InlineData(0, 10, 11, false)]
    public void Contains_ShouldWorkCorrectly(int min, int max, int testValue, bool expected)
    {
        var range = PackRange.Of(min, max);
        Assert.Equal(expected, range.Contains(testValue));
    }

    [Fact]
    public void ToString_ShouldFormatCorrectly()
    {
        var range = PackRange.Of(5, 10);
        Assert.Equal("[5..10]", range.ToString());
    }

    [Fact]
    public void Equality_ShouldWorkCorrectly()
    {
        var range1 = PackRange.Of(5, 10);
        var range2 = PackRange.Of(5, 10);
        var range3 = PackRange.Of(0, 10);

        Assert.Equal(range1, range2);
        Assert.True(range1 == range2);
        Assert.NotEqual(range1, range3);
        Assert.True(range1 != range3);
    }

}
