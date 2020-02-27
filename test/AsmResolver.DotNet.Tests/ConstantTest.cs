using System.IO;
using System.Linq;
using AsmResolver.DotNet.TestCases.Fields;
using Xunit;

namespace AsmResolver.DotNet.Tests
{
    public class ConstantTest
    {
        private Constant GetFieldConstant(string name)
        {
            var module = ModuleDefinition.FromFile(typeof(Constants).Assembly.Location);
            return GetFieldConstantInModule(module, name);
        }

        private static Constant GetFieldConstantInModule(ModuleDefinition module, string name)
        {
            var t = module.TopLevelTypes.First(t => t.Name == nameof(Constants));
            return t.Fields.First(f => f.Name == name).Constant;
        }

        private Constant RebuildAndLookup(ModuleDefinition module, string name)
        {
            string tempFile = Path.GetTempFileName();
            module.Write(tempFile);
            
            var stream = new MemoryStream();
            module.Write(stream);
            
            var newModule = ModuleDefinition.FromBytes(stream.ToArray());
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
        public void ReadAndInterpretData(string name, object expected)
        {
            var constant = GetFieldConstant(name);
            Assert.Equal(expected, constant.Value.InterpretData(constant.Type));
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
        public void PersistentConstants(string name)
        {
            var constant = GetFieldConstant(name);
            var newConstant = RebuildAndLookup(constant.Parent.Module, name);
            Assert.NotNull(newConstant);
            Assert.Equal(constant.Value.Data, newConstant.Value.Data);
        }
        
    }
}