using System.Linq;
using AsmResolver.DotNet.Resources;
using Xunit;

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
    }
}
