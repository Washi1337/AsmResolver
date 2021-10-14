using System.IO;
using System.Linq;
using System.Resources;
using AsmResolver.DotNet.Resources;
using AsmResolver.IO;
using Xunit;
using ResourceSet = AsmResolver.DotNet.Resources.ResourceSet;

namespace AsmResolver.DotNet.Tests.Resources
{
    public class ResourceSetTest
    {
        private static readonly ManifestResource Resource;

        static ResourceSetTest()
        {
            var module = ModuleDefinition.FromFile(typeof(TestCases.Resources.Resources).Assembly.Location);
            Resource = module.Resources.First(r =>
                r.Name == "AsmResolver.DotNet.TestCases.Resources.Properties.Resources.resources");
        }

        public static ResourceSet ReadResourceSet()
        {
            Assert.True(Resource.TryGetReader(out var reader));
            return ResourceSet.FromReader(reader);
        }

        [Theory]
        [InlineData("Null", ResourceTypeCode.Null, null)]
        [InlineData("String", ResourceTypeCode.String, "Hello, world!")]
        [InlineData("BoolFalse", ResourceTypeCode.Boolean, false)]
        [InlineData("BoolTrue", ResourceTypeCode.Boolean, true)]
        [InlineData("Char", ResourceTypeCode.Char, 'a')]
        [InlineData("Byte", ResourceTypeCode.Byte, (byte) 0x12)]
        [InlineData("SByte", ResourceTypeCode.SByte, (sbyte) 0x12)]
        [InlineData("UInt16", ResourceTypeCode.UInt16, (ushort) 0x1234)]
        [InlineData("Int16", ResourceTypeCode.Int16, (short) 0x1234)]
        [InlineData("UInt32", ResourceTypeCode.UInt32, (uint) 0x12345678)]
        [InlineData("Int32", ResourceTypeCode.Int32, (int) 0x12345678)]
        [InlineData("UInt64", ResourceTypeCode.UInt64, (ulong) 0x123456789abcdef)]
        [InlineData("Int64", ResourceTypeCode.Int64, (long) 0x123456789abcdef)]
        [InlineData("Single", ResourceTypeCode.Single, 1.234f)]
        [InlineData("Double", ResourceTypeCode.Double, 1.234D)]
        public void ReadIntrinsicElement(string key, ResourceTypeCode expectedType, object expectedValue)
        {
            var entry = ReadResourceSet().First(e => e.Name == key);
            Assert.Equal(IntrinsicResourceType.Get(expectedType), entry.Type);
            Assert.Equal(expectedValue, entry.Data);
        }

        [Theory]
        [InlineData("Null", ResourceTypeCode.Null, null)]
        [InlineData("String", ResourceTypeCode.String, "Hello, world!")]
        [InlineData("BoolFalse", ResourceTypeCode.Boolean, false)]
        [InlineData("BoolTrue", ResourceTypeCode.Boolean, true)]
        [InlineData("Char", ResourceTypeCode.Char, 'a')]
        [InlineData("Byte", ResourceTypeCode.Byte, (byte) 0x12)]
        [InlineData("SByte", ResourceTypeCode.SByte, (sbyte) 0x12)]
        [InlineData("UInt16", ResourceTypeCode.UInt16, (ushort) 0x1234)]
        [InlineData("Int16", ResourceTypeCode.Int16, (short) 0x1234)]
        [InlineData("UInt32", ResourceTypeCode.UInt32, (uint) 0x12345678)]
        [InlineData("Int32", ResourceTypeCode.Int32, (int) 0x12345678)]
        [InlineData("UInt64", ResourceTypeCode.UInt64, (ulong) 0x123456789abcdef)]
        [InlineData("Int64", ResourceTypeCode.Int64, (long) 0x123456789abcdef)]
        [InlineData("Single", ResourceTypeCode.Single, 1.234f)]
        [InlineData("Double", ResourceTypeCode.Double, 1.234D)]
        public void PersistentIntrinsicElement(string key, ResourceTypeCode type, object value)
        {
            var set = new ResourceSet();
            var entry = new ResourceSetEntry(key, type, value);
            set.Add(entry);

            using var stream = new MemoryStream();
            set.Write(new BinaryStreamWriter(stream));

            var actualSet = ResourceSet.FromReader(ByteArrayDataSource.CreateReader(stream.ToArray()));
            var actualEntry = actualSet.First(e => e.Name == key);
            Assert.Equal(entry.Type, actualEntry.Type);
            Assert.Equal(entry.Data, actualEntry.Data);
        }

        [Fact]
        public void PersistentSetMultipleEntries()
        {
            var set = new ResourceSet
            {
                new("Lorem", ResourceTypeCode.ByteArray, new byte[] {1, 2, 3, 4, 5, 6}),
                new("Ipsum", ResourceTypeCode.ByteArray, new byte[] {7, 8, 9, 10}),
                new("Dolor", ResourceTypeCode.ByteArray, new byte[] {11, 12, 13, 14, 15, 16, 17}),
                new("Sit", ResourceTypeCode.ByteArray, new byte[] {18}),
                new("Amet", ResourceTypeCode.ByteArray, new byte[] {19, 20, 21, 22, 23}),
            };

            using var stream = new MemoryStream();
            set.Write(new BinaryStreamWriter(stream));
            stream.Position = 0;

            var resourceReader = new ResourceReader(stream);

            var actualSet = ResourceSet.FromReader(ByteArrayDataSource.CreateReader(stream.ToArray()));
            Assert.Equal(set.Count, actualSet.Count);
            for (int i = 0; i < set.Count; i++)
            {
                var entry = set[i];
                var actualEntry = actualSet.First(x => x.Name == entry.Name);

                // Verify type and contents.
                Assert.Equal(entry.Type, actualEntry.Type);
                Assert.Equal(entry.Data, actualEntry.Data);

                // Verify default resource reader can still get to the data (== hash table verification).
                resourceReader.GetResourceData(entry.Name, out string type, out byte[] data);
            }
        }
    }
}
