# CompactPack

A high-performance .NET library for efficient bit packing that allows you to store multiple values in a single integer with minimal memory usage.

[![.NET](https://img.shields.io/badge/.NET-Standard2.0%2B-purple)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![NuGet](https://img.shields.io/nuget/v/CompactPack.svg)](https://www.nuget.org/packages/CompactPack)

## 🚀 Why CompactPack?

- **Space Efficient**: Pack multiple values into a single integer, saving up to 75% memory
- **Type Safe**: Strongly typed with compile-time validation
- **Flexible**: Support for BigInteger (unlimited), 64-bit, 32-bit, and 256-bit storage
- **Range-Based**: Define value ranges and let CompactPack calculate optimal bit usage

## 📦 Installation

```bash
dotnet add package CompactPack
```

## 🎯 Quick Start

```csharp
using CompactPack;
using CompactPack.Packers;

// Create a packer for game character stats
var characterPacker = new BitPacker<BigInteger>()
    .AddField("Health", PackRange.Of(1000))      // 0-1000 HP (10 bits)
    .AddField("Level", PackRange.Of(1, 100))     // 1-100 (7 bits)  
    .AddField("Experience", PackRange.Of(999999)) // 0-999999 XP (20 bits)
    .AddField("Class", PackRange.Of(15));        // 0-15 classes (4 bits)

// Set values
characterPacker.SetValue("Health", 750)
               .SetValue("Level", 42)
               .SetValue("Experience", 125000)
               .SetValue("Class", 3);

// Pack into single integer (41 bits total = 6 bytes instead of 16 bytes)
var packed = characterPacker.Pack();

// Unpack later - recreate the same packer structure
var unpacker = new BitPacker<BigInteger>()
    .AddField("Health", PackRange.Of(1000))
    .AddField("Level", PackRange.Of(1, 100))
    .AddField("Experience", PackRange.Of(999999))
    .AddField("Class", PackRange.Of(15))
    .Unpack(packed);

Console.WriteLine($"Health: {unpacker.GetValue("Health")}");     // 750
Console.WriteLine($"Level: {unpacker.GetValue("Level")}");       // 42
Console.WriteLine($"Experience: {unpacker.GetValue("Experience")}"); // 125000
Console.WriteLine($"Class: {unpacker.GetValue("Class")}");       // 3
```

## 📊 Core Concepts

### PackRange - Define Value Ranges

```csharp
// Zero-based ranges (0 to max)
PackRange.Of(255)        // 0-255 (8 bits)
PackRange.Of(1000)       // 0-1000 (10 bits)

// Custom ranges  
PackRange.Of(1, 100)     // 1-100 (7 bits)
PackRange.Of(-40, 85)    // -40°C to 85°C (7 bits)

// Range properties
var range = PackRange.Of(10, 50);
Console.WriteLine(range.BitsRequired);  // 6 bits
Console.WriteLine(range.ValueCount);    // 41 values  
Console.WriteLine(range.Contains(25));  // true
```

### Field Types

```csharp
var packer = new BitPacker<BigInteger>();

// By range (recommended - optimal bit usage)
packer.AddField("temperature", PackRange.Of(-40, 85));

// By explicit bit width
packer.AddField("flags", 8); // Exactly 8 bits

// By byte count (when you need specific storage)
packer.AddFieldWithBytes("data", 4); // Exactly 4 bytes = 32 bits
```

## 🎮 Real-World Examples

### Game Character Coordinates

```csharp
var coordinatePacker = Bit32SignedPacker.Create()
    .AddField("X", PackRange.Of(-2048, 2047))     // World X coordinate
    .AddField("Y", PackRange.Of(-2048, 2047))     // World Y coordinate  
    .AddField("Z", PackRange.Of(-512, 511))       // World Z coordinate
    .AddField("Rotation", PackRange.Of(0, 359));  // Rotation in degrees

// Pack player position and rotation into single int
var position = coordinatePacker.SetValue("X", -1500)
                               .SetValue("Y", 750)
                               .SetValue("Z", -100)
                               .SetValue("Rotation", 270)
                               .Pack();
```

### Game Character Stats

```csharp
var playerPacker = new BitPacker<BigInteger>()
    .AddField("Health", PackRange.Of(1000))
    .AddField("Mana", PackRange.Of(500))  
    .AddField("Level", PackRange.Of(1, 100))
    .AddField("Experience", PackRange.Of(10000000))
    .AddField("Gold", PackRange.Of(999999))
    .AddField("Class", PackRange.Of(7))     // 8 classes (0-7)
    .AddField("Race", PackRange.Of(4))      // 5 races (0-4)
    .AddField("PvpEnabled", PackRange.Of(1)); // boolean

// Total: ~44 bits instead of 64 bytes (8 fields × 8 bytes each)
Console.WriteLine($"Space usage: {playerPacker.TotalBytesNeeded} bytes");
```

### IoT Sensor Data

```csharp
var sensorPacker = Bit64Packer.Create()
    .AddField("Temperature", PackRange.Of(-40, 125))  // -40°C to 125°C
    .AddField("Humidity", PackRange.Of(0, 100))       // 0-100%
    .AddField("Pressure", PackRange.Of(300, 1200))    // 300-1200 hPa
    .AddField("BatteryLevel", PackRange.Of(0, 100))   // 0-100%
    .AddField("SensorId", PackRange.Of(255))          // 256 sensors
    .AddField("Timestamp", PackRange.Of(4294967295)); // Unix timestamp

// Fits in single 64-bit integer instead of 24 bytes
var readings = sensorPacker
    .SetValue("Temperature", 23)
    .SetValue("Humidity", 65)
    .SetValue("Pressure", 1013)
    .SetValue("BatteryLevel", 78)
    .SetValue("SensorId", 42)
    .SetValue("Timestamp", 1703980800)
    .Pack();
```

### DNA/Genetics Data

```csharp
var dnaPacker = Bit256Packer.Create();

// Add 10 genetic parts, each with 4 traits (6 bits each)
for (int i = 0; i < 10; i++)
{
    dnaPacker.AddField($"Part{i}_Trait1", PackRange.Of(63))
             .AddField($"Part{i}_Trait2", PackRange.Of(63))
             .AddField($"Part{i}_Trait3", PackRange.Of(63))
             .AddField($"Part{i}_Trait4", PackRange.Of(63));
}

// 240 bits total (fits perfectly in 256-bit storage)
Console.WriteLine($"DNA storage: {dnaPacker.TotalBitWidth}/256 bits");

// Set genetic data
for (int i = 0; i < 10; i++)
{
    dnaPacker.SetValue($"Part{i}_Trait1", Random.Shared.Next(64))
             .SetValue($"Part{i}_Trait2", Random.Shared.Next(64))
             .SetValue($"Part{i}_Trait3", Random.Shared.Next(64))
             .SetValue($"Part{i}_Trait4", Random.Shared.Next(64));
}

var dnaSequence = dnaPacker.Pack();
```

## 🏗️ Specialized Packers

CompactPack provides specialized packers for different storage needs:

- **Bit32Packer** - 32-bit unsigned storage (uint) - 32 bits capacity
- **Bit32SignedPacker** - 32-bit signed storage (int) - 31 bits capacity  
- **Bit64Packer** - 64-bit unsigned storage (ulong) - 64 bits capacity
- **Bit64SignedPacker** - 64-bit signed storage (long) - 63 bits capacity
- **Bit256Packer** - 256-bit storage (BigInteger) - 256 bits capacity
- **UnlimitedBitPacker** - Unlimited storage (BigInteger) - no capacity limit

### Bit32Packer - 32-bit Storage

```csharp
// Perfect for RGBA colors, flags, small datasets
var colorPacker = Bit32Packer.Create()
    .AddField("Red", PackRange.Of(255))
    .AddField("Green", PackRange.Of(255))  
    .AddField("Blue", PackRange.Of(255))
    .AddField("Alpha", PackRange.Of(255));

var color = colorPacker.SetValue("Red", 255)
                       .SetValue("Green", 128)
                       .SetValue("Blue", 64) 
                       .SetValue("Alpha", 200)
                       .Pack(); // Returns uint

Console.WriteLine($"RGBA: 0x{color:X8}");
```

### Bit32SignedPacker - 32-bit Signed Storage

```csharp
// Perfect for signed values, coordinates, deltas
var deltaPacker = Bit32SignedPacker.Create()
    .AddField("X", PackRange.Of(-1000, 1000))    // Position delta
    .AddField("Y", PackRange.Of(-1000, 1000))    // Position delta
    .AddField("Temperature", PackRange.Of(-40, 85)); // Celsius

var delta = deltaPacker.SetValue("X", -500)
                       .SetValue("Y", 750)
                       .SetValue("Temperature", 23)
                       .Pack(); // Returns int
```

### Bit64Packer - 64-bit Storage

```csharp
// Great for timestamps, IDs, medium datasets  
var eventPacker = Bit64Packer.Create()
    .AddField("Timestamp", PackRange.Of(4294967295L)) // 32 bits
    .AddField("UserId", PackRange.Of(1048575))        // 20 bits  
    .AddField("EventType", PackRange.Of(15))          // 4 bits
    .AddField("SessionId", PackRange.Of(255));        // 8 bits

var eventData = eventPacker.SetValue("Timestamp", DateTimeOffset.Now.ToUnixTimeSeconds())
                           .SetValue("UserId", 12345)
                           .SetValue("EventType", 3)
                           .SetValue("SessionId", 78)
                           .Pack(); // Returns ulong
```

### Bit64SignedPacker - 64-bit Signed Storage

```csharp
// Perfect for large signed ranges, financial data
var financialPacker = Bit64SignedPacker.Create()
    .AddField("Balance", PackRange.Of(-1000000000L, 1000000000L)) // Account balance
    .AddField("TransactionId", PackRange.Of(0, 1048575))          // Transaction ID
    .AddField("UserId", PackRange.Of(0, 65535));                  // User ID

var financial = financialPacker.SetValue("Balance", -50000)
                               .SetValue("TransactionId", 12345)
                               .SetValue("UserId", 9876)
                               .Pack(); // Returns long
```

### Bit256Packer - 256-bit Storage

```csharp
// Perfect for blockchain, cryptography, large datasets
var cryptoPacker = Bit256Packer.CreateEthereumStyle(); // Pre-configured

cryptoPacker.SetValue("Address", BigInteger.Parse("1461501637330902918203684832716283019655932542975"))
            .SetValue("Value", 1000000000000000000) // 1 ETH in wei
            .SetValue("Nonce", 42);

var transaction = cryptoPacker.Pack(); // Returns BigInteger

// Custom 256-bit packer
var hashPacker = Bit256Packer.CreateHashStyle()
    .SetValue("Hash", yourBigInteger256BitHash)
    .Pack();
```

### UnlimitedBitPacker - BigInteger Storage

```csharp
// No size limits - use as many bits as needed
var unlimitedPacker = UnlimitedBitPacker.Create()
    .AddField("VeryLargeNumber", PackRange.Of(BigInteger.Parse("123456789012345678901234567890")))
    .AddField("AnotherLargeNumber", PackRange.Of(BigInteger.Parse("987654321098765432109876543210")));

// Can store arbitrarily large values
```

## 🔧 Advanced Features

### Multiple Fields at Once

```csharp
// Add multiple fields with same bit width
packer.AddFields(6, "Strength", "Dexterity", "Constitution", "Intelligence", "Wisdom", "Charisma");

// Add multiple fields with same range
packer.AddFields(PackRange.Of(20), "Skill1", "Skill2", "Skill3", "Skill4");
```

### Unpacking Data

```csharp
// Method 1: Manual recreation (when you know the structure)
var unpacker = new BitPacker<BigInteger>()
    .AddField("Health", PackRange.Of(1000))
    .AddField("Level", PackRange.Of(1, 100))
    .AddField("Experience", PackRange.Of(999999))
    .AddField("Class", PackRange.Of(15))
    .Unpack(packedData);

// Method 2: Using CreateSimilar (copies field structure)
var unpacker2 = originalPacker.CreateSimilar().Unpack(packedData);

// Method 3: Reuse the same packer instance
originalPacker.Unpack(packedData); // Overwrites current values
var health = originalPacker.GetValue("Health");
```

### Field Information and Validation

```csharp
var field = packer.GetFieldInfo("Health");
Console.WriteLine($"Field: {field.Name}");
Console.WriteLine($"Bit offset: {field.BitOffset}");  
Console.WriteLine($"Bit width: {field.BitWidth}");
Console.WriteLine($"Range: {field.MinValue}-{field.MaxValue}");

// Check if value is valid
field.ValidateValue(750); // Throws if invalid

// Get all fields
foreach (var f in packer.Fields)
{
    Console.WriteLine($"{f.Name}: {f.BitWidth} bits at offset {f.BitOffset}");
}
```

### Storage Analysis

```csharp
Console.WriteLine($"Total bits used: {packer.TotalBitWidth}");
Console.WriteLine($"Total bytes needed: {packer.TotalBytesNeeded}"); 
Console.WriteLine($"Fits in storage type: {packer.FitsInStorageType()}");

// For 256-bit packer
var packer256 = Bit256Packer.Create();
Console.WriteLine($"Remaining capacity: {packer256.RemainingBits} bits");
Console.WriteLine($"Can fit 8-bit field: {packer256.CanFitField(8)}");
Console.WriteLine($"Max value for remaining bits: {packer256.MaxValueForRemainingBits}");
```

## 📈 Memory Benefits

| Scenario | Traditional Storage | CompactPack | Savings |
|----------|-------------------|-------------|---------|
| RGB Color (3×8 bits) | 12 bytes (3×int) | 4 bytes (uint) | 67% |
| Game Coordinates (4 signed values) | 16 bytes (4×int) | 4 bytes (int) | 75% |
| Game Stats (8 fields) | 64 bytes (8×long) | 6 bytes | 91% |  
| Sensor Data (6 values) | 24 bytes | 8 bytes | 67% |
| Financial Data (3 signed longs) | 24 bytes (3×long) | 8 bytes (long) | 67% |
| DNA Sequence (40 traits) | 160 bytes | 30 bytes | 81% |

## 🛡️ Error Handling

```csharp
try 
{
    packer.SetValue("Health", 1500); // Exceeds max of 1000
}
catch (ArgumentOutOfRangeException ex)
{
    Console.WriteLine($"Invalid value: {ex.Message}");
}

try
{
    packer.AddField("Health", 8); // Duplicate field name  
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Field error: {ex.Message}");
}
```

## 🧪 Testing

CompactPack includes comprehensive tests covering:
- Round-trip pack/unpack operations  
- Range validation and edge cases
- Large value handling (BigInteger)
- Storage type limits and overflow detection

```bash
dotnet test
```

## 📋 Requirements

- .NET Standard 2.0 or higher
- System.Numerics (included in .NET)

## 🤝 Contributing

Contributions are welcome! Please feel free to submit pull requests, report bugs, or suggest features.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Inspired by bit manipulation techniques used in game engines and embedded systems
- Designed for modern .NET applications requiring efficient data storage
- Perfect for blockchain, IoT, gaming, and high-performance applications

---

**CompactPack**: *Making every bit count* ⚡