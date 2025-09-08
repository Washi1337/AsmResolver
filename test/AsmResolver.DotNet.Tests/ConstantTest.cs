using System;
using System.IO;
using System.Linq;
using AsmResolver.DotNet.TestCases.Fields;
using AsmResolver.PE.DotNet.Metadata.Tables;
using Xunit;

namespace AsmResolver.DotNet.Tests
{
    public partial class ConstantTest
    {
        private Constant GetFieldConstant(string name)
        {
            var module = ModuleDefinition.FromFile(typeof(Constants).Assembly.Location, TestReaderParameters);
            return GetFieldConstantInModule(module, name);
        }

        private static Constant GetFieldConstantInModule(ModuleDefinition module, string name)
        {
            var t = module.TopLevelTypes.First(t => t.Name == nameof(Constants));
            return t.Fields.First(f => f.Name == name).Constant;
        }

        private Constant RebuildAndLookup(ModuleDefinition module, string name)
        {
            var stream = new MemoryStream();
            module.Write(stream);

            var newModule = ModuleDefinition.FromBytes(stream.ToArray(), TestReaderParameters);
            return GetFieldConstantInModule(newModule, name);
        }

        [Theory]
        [InlineData(nameof(Constants.Boolean), Constants.Boolean)]
        [InlineData(nameof(Constants.Byte), Constants.Byte)]
        [InlineData(nameof(Constants.UInt16), Constants.UInt16)]
        [InlineData(nameof(Constants.UInt32), Constants.UInt32)]
        [InlineData(nameof(Constants.UInt64), Constants.UInt64)]
        [InlineData(nameof(Constants.SByte), Constants.SByte)]
        [InlineData(nameof(Constants.Int16), Constants.Int16)]
        [InlineData(nameof(Constants.Int32), Constants.Int32)]
        [InlineData(nameof(Constants.Int64), Constants.Int64)]
        [InlineData(nameof(Constants.Single), Constants.Single)]
        [InlineData(nameof(Constants.Double), Constants.Double)]
        [InlineData(nameof(Constants.Char), Constants.Char)]
        [InlineData(nameof(Constants.String), Constants.String)]
        [InlineData(nameof(Constants.NullString), Constants.NullString)]
        public void ReadAndInterpretData(string name, object expected)
        {
            var constant = GetFieldConstant(name);
            Assert.Equal(expected, constant.InterpretData());
        }

        [Theory]
        [InlineData(nameof(Constants.Boolean))]
        [InlineData(nameof(Constants.Byte))]
        [InlineData(nameof(Constants.UInt16))]
        [InlineData(nameof(Constants.UInt32))]
        [InlineData(nameof(Constants.UInt64))]
        [InlineData(nameof(Constants.SByte))]
        [InlineData(nameof(Constants.Int16))]
        [InlineData(nameof(Constants.Int32))]
        [InlineData(nameof(Constants.Int64))]
        [InlineData(nameof(Constants.Single))]
        [InlineData(nameof(Constants.Double))]
        [InlineData(nameof(Constants.Char))]
        [InlineData(nameof(Constants.String))]
        [InlineData(nameof(Constants.NullString))]
        public void PersistentConstants(string name)
        {
            var constant = GetFieldConstant(name);
            var newConstant = RebuildAndLookup(constant.Parent!.DeclaringModule, name);
            Assert.NotNull(newConstant);
            Assert.Equal(constant.Value.Data, newConstant.Value.Data);
        }

        [Fact]
        public void ReadInvalidConstantValueShouldNotThrow()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.ConstantZeroValueColumn, TestReaderParameters);
            var constantValue = module
                .TopLevelTypes.First(t => t.Name == "MyClass")
                .Fields.First(f => f.Name == "MyIntegerConstant")
                .Constant.Value;
            Assert.Null(constantValue);
        }

        [Fact]
        public void WriteNullConstantValueShouldNotThrow()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.ConstantZeroValueColumn, TestReaderParameters);

            var stream = new MemoryStream();
            module.Write(stream);

            var newModule = ModuleDefinition.FromBytes(stream.ToArray(), TestReaderParameters);

            var constantValue = newModule
                .TopLevelTypes.First(t => t.Name == "MyClass")
                .Fields.First(f => f.Name == "MyIntegerConstant")
                .Constant.Value;
            Assert.Null(constantValue);
        }

        [Theory]
        [InlineData(ElementType.Boolean)]
        [InlineData(ElementType.Char)]
        [InlineData(ElementType.I1)]
        [InlineData(ElementType.U1)]
        [InlineData(ElementType.I2)]
        [InlineData(ElementType.U2)]
        [InlineData(ElementType.I4)]
        [InlineData(ElementType.U4)]
        [InlineData(ElementType.I)]
        [InlineData(ElementType.U)]
        [InlineData(ElementType.I8)]
        [InlineData(ElementType.U8)]
        [InlineData(ElementType.R4)]
        [InlineData(ElementType.R8)]
        [InlineData(ElementType.String)]
        public void DefaultConstantHasDataEqualToZero(ElementType elementType)
        {
            ModuleDefinition module = new("Module");
            var type = module.CorLibTypeFactory.FromElementType(elementType);
            var constant = Constant.FromDefault(type);
            object value = constant.InterpretData();
            if (elementType is ElementType.String)
            {
                Assert.Null(value);
            }
            else
            {
                Assert.IsType<IConvertible>(value, exactMatch: false);
                Assert.Equal(0, ((IConvertible)value).ToInt32(null));
            }
        }
    }
}
