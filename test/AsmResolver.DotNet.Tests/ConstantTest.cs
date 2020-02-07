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
            var t = module.TopLevelTypes.First(t => t.Name == nameof(Constants));
            return t.Fields.First(f => f.Name == name).Constant;
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

    }
}