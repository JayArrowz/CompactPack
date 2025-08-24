using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using CompactPack.Benchmarks.Models;
using CompactPack.Packers;
using System.Numerics;

namespace CompactPack.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class PackingBenchmarks
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private BitPacker<BigInteger> _gameStatsPacker;
    private Bit32Packer _colorPacker;
    private Bit64Packer _sensorPacker;
    private Bit256Packer _dnaPacker;
    private UnlimitedBitPacker _unlimitedPacker;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    private BigInteger _gameStatsPackedValue;
    private uint _colorPackedValue;
    private ulong _sensorPackedValue;
    private BigInteger _dnaPackedValue;

    [GlobalSetup]
    public void Setup()
    {
        // Game Stats Packer
        _gameStatsPacker = new BitPacker<BigInteger>()
            .AddField("Health", PackRange.Of(1000))
            .AddField("Level", PackRange.Of(1, 100))
            .AddField("Experience", PackRange.Of(999999))
            .AddField("Class", PackRange.Of(15));

        // Color Packer
        _colorPacker = Bit32Packer.Create()
            .AddField("Red", PackRange.Of(255))
            .AddField("Green", PackRange.Of(255))
            .AddField("Blue", PackRange.Of(255))
            .AddField("Alpha", PackRange.Of(255));

        // Sensor Packer
        _sensorPacker = Bit64Packer.Create()
            .AddField("Temperature", PackRange.Of(-40, 125))
            .AddField("Humidity", PackRange.Of(0, 100))
            .AddField("Pressure", PackRange.Of(300, 1200))
            .AddField("BatteryLevel", PackRange.Of(0, 100))
            .AddField("SensorId", PackRange.Of(255))
            .AddField("Timestamp", PackRange.Of(65535)); // Reduced to fit in 64 bits

        // DNA Packer (simplified for benchmarking)
        _dnaPacker = Bit256Packer.Create();
        for (int i = 0; i < 10; i++)
        {
            _dnaPacker.AddField($"Part{i}_P1", PackRange.Of(63))
                     .AddField($"Part{i}_H1", PackRange.Of(63))
                     .AddField($"Part{i}_H2", PackRange.Of(63))
                     .AddField($"Part{i}_H3", PackRange.Of(63));
        }

        // Unlimited Packer
        _unlimitedPacker = UnlimitedBitPacker.Create()
            .AddField("LargeValue1", PackRange.Of(BigInteger.Parse("999999999999")))
            .AddField("LargeValue2", PackRange.Of(BigInteger.Parse("888888888888")));

        // Pre-pack values for unpacking benchmarks
        _gameStatsPackedValue = _gameStatsPacker.SetValue("Health", 750)
                                               .SetValue("Level", 42)
                                               .SetValue("Experience", 125000)
                                               .SetValue("Class", 3)
                                               .Pack();

        _colorPackedValue = _colorPacker.SetValue("Red", 255)
                                       .SetValue("Green", 128)
                                       .SetValue("Blue", 64)
                                       .SetValue("Alpha", 200)
                                       .Pack();

        _sensorPackedValue = _sensorPacker.SetValue("Temperature", 23)
                                         .SetValue("Humidity", 65)
                                         .SetValue("Pressure", 1013)
                                         .SetValue("BatteryLevel", 78)
                                         .SetValue("SensorId", 42)
                                         .SetValue("Timestamp", 12345)
                                         .Pack();

        var tempDnaPacker = _dnaPacker.CreateSimilar();
        for (int i = 0; i < 10; i++)
        {
            tempDnaPacker.SetValue($"Part{i}_P1", 32)
                        .SetValue($"Part{i}_H1", 16)
                        .SetValue($"Part{i}_H2", 48)
                        .SetValue($"Part{i}_H3", 8);
        }
        _dnaPackedValue = tempDnaPacker.Pack();
    }

    #region Packing Benchmarks

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("GameStats")]
    public GameStats TraditionalGameStats_Create()
    {
        return new GameStats { Health = 750, Level = 42, Experience = 125000, Class = 3 };
    }

    [Benchmark]
    [BenchmarkCategory("GameStats")]
    public BigInteger CompactPack_GameStats_Pack()
    {
        return _gameStatsPacker.SetValue("Health", 750)
                              .SetValue("Level", 42)
                              .SetValue("Experience", 125000)
                              .SetValue("Class", 3)
                              .Pack();
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Color")]
    public RgbaColor TraditionalColor_Create()
    {
        return new RgbaColor { Red = 255, Green = 128, Blue = 64, Alpha = 200 };
    }

    [Benchmark]
    [BenchmarkCategory("Color")]
    public uint CompactPack_Color_Pack()
    {
        return _colorPacker.SetValue("Red", 255)
                          .SetValue("Green", 128)
                          .SetValue("Blue", 64)
                          .SetValue("Alpha", 200)
                          .Pack();
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Sensor")]
    public SensorReading TraditionalSensor_Create()
    {
        return new SensorReading { Temperature = 23, Humidity = 65, Pressure = 1013, BatteryLevel = 78, SensorId = 42, Timestamp = 12345 };
    }

    [Benchmark]
    [BenchmarkCategory("Sensor")]
    public ulong CompactPack_Sensor_Pack()
    {
        return _sensorPacker.SetValue("Temperature", 23)
                           .SetValue("Humidity", 65)
                           .SetValue("Pressure", 1013)
                           .SetValue("BatteryLevel", 78)
                           .SetValue("SensorId", 42)
                           .SetValue("Timestamp", 12345)
                           .Pack();
    }

    #endregion

    #region Unpacking Benchmarks

    [Benchmark]
    [BenchmarkCategory("GameStats")]
    public int CompactPack_GameStats_Unpack()
    {
        var unpacker = _gameStatsPacker.CreateSimilar().Unpack(_gameStatsPackedValue);
        return unpacker.GetValueAsInt("Health");
    }

    [Benchmark]
    [BenchmarkCategory("Color")]
    public int CompactPack_Color_Unpack()
    {
        var unpacker = _colorPacker.CreateSimilar().Unpack(_colorPackedValue);
        return unpacker.GetValueAsInt("Red");
    }

    [Benchmark]
    [BenchmarkCategory("Sensor")]
    public int CompactPack_Sensor_Unpack()
    {
        var unpacker = _sensorPacker.CreateSimilar().Unpack(_sensorPackedValue);
        return unpacker.GetValueAsInt("Temperature");
    }

    #endregion

    #region Memory Usage Benchmarks

    [Benchmark]
    [BenchmarkCategory("Memory")]
    public object[] TraditionalObjects_Array()
    {
        return new object[]
        {
            new GameStats { Health = 750, Level = 42, Experience = 125000, Class = 3 },
            new GameStats { Health = 800, Level = 45, Experience = 130000, Class = 1 },
            new GameStats { Health = 650, Level = 38, Experience = 95000, Class = 2 }
        };
    }

    [Benchmark]
    [BenchmarkCategory("Memory")]
    public BigInteger[] CompactPack_Array()
    {
        var packer = _gameStatsPacker.CreateSimilar();
        return new BigInteger[]
        {
            packer.SetValue("Health", 750).SetValue("Level", 42).SetValue("Experience", 125000).SetValue("Class", 3).Pack(),
            packer.SetValue("Health", 800).SetValue("Level", 45).SetValue("Experience", 130000).SetValue("Class", 1).Pack(),
            packer.SetValue("Health", 650).SetValue("Level", 38).SetValue("Experience", 95000).SetValue("Class", 2).Pack()
        };
    }

    #endregion

    #region Specialized Packer Benchmarks

    [Benchmark]
    [BenchmarkCategory("Specialized")]
    public BigInteger Bit256Packer_DNA_Pack()
    {
        var packer = _dnaPacker.CreateSimilar();
        for (int i = 0; i < 10; i++)
        {
            packer.SetValue($"Part{i}_P1", 32)
                  .SetValue($"Part{i}_H1", 16)
                  .SetValue($"Part{i}_H2", 48)
                  .SetValue($"Part{i}_H3", 8);
        }
        return packer.Pack();
    }

    [Benchmark]
    [BenchmarkCategory("Specialized")]
    public int Bit256Packer_DNA_Unpack()
    {
        var unpacker = _dnaPacker.CreateSimilar().Unpack(_dnaPackedValue);
        return unpacker.GetValueAsInt("Part0_P1");
    }

    [Benchmark]
    [BenchmarkCategory("Specialized")]
    public BigInteger UnlimitedPacker_Pack()
    {
        return _unlimitedPacker.SetValue("LargeValue1", BigInteger.Parse("123456789123"))
                              .SetValue("LargeValue2", BigInteger.Parse("888888888555"))
                              .Pack();
    }

    #endregion

    #region Reuse vs Recreation Benchmarks

    [Benchmark]
    [BenchmarkCategory("Reuse")]
    public uint Bit32Packer_Reuse_Same_Instance()
    {
        // Reuse the same packer instance (should be more efficient)
        return _colorPacker.SetValue("Red", 200)
                          .SetValue("Green", 100)
                          .SetValue("Blue", 50)
                          .SetValue("Alpha", 255)
                          .Pack();
    }

    [Benchmark]
    [BenchmarkCategory("Reuse")]
    public uint Bit32Packer_Create_New_Instance()
    {
        // Create new instance each time (should allocate more)
        var packer = Bit32Packer.Create()
            .AddField("Red", PackRange.Of(255))
            .AddField("Green", PackRange.Of(255))
            .AddField("Blue", PackRange.Of(255))
            .AddField("Alpha", PackRange.Of(255));

        return packer.SetValue("Red", 200)
                     .SetValue("Green", 100)
                     .SetValue("Blue", 50)
                     .SetValue("Alpha", 255)
                     .Pack();
    }

    [Benchmark]
    [BenchmarkCategory("Reuse")]
    public uint Bit32Packer_CreateSimilar()
    {
        // Use CreateSimilar (middle ground)
        return _colorPacker.CreateSimilar()
                          .SetValue("Red", 200)
                          .SetValue("Green", 100)
                          .SetValue("Blue", 50)
                          .SetValue("Alpha", 255)
                          .Pack();
    }

    #endregion

    #region Range vs BitWidth Benchmarks

    [Benchmark]
    [BenchmarkCategory("FieldTypes")]
    public BigInteger Range_Based_Fields()
    {
        var packer = new BitPacker<BigInteger>()
            .AddField("Value1", PackRange.Of(255))      // Optimal 8 bits
            .AddField("Value2", PackRange.Of(1023))     // Optimal 10 bits
            .AddField("Value3", PackRange.Of(15));      // Optimal 4 bits

        return packer.SetValue("Value1", 200)
                     .SetValue("Value2", 500)
                     .SetValue("Value3", 10)
                     .Pack();
    }

    [Benchmark]
    [BenchmarkCategory("FieldTypes")]
    public BigInteger BitWidth_Based_Fields()
    {
        var packer = new BitPacker<BigInteger>()
            .AddField("Value1", 8)    // Explicit 8 bits
            .AddField("Value2", 16)   // Explicit 16 bits (wastes 6 bits)
            .AddField("Value3", 8);   // Explicit 8 bits (wastes 4 bits)

        return packer.SetValue("Value1", 200)
                     .SetValue("Value2", 500)
                     .SetValue("Value3", 10)
                     .Pack();
    }

    #endregion
}
