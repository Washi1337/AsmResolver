using AsmResolver.DotNet.TestCases.Fields;
using Xunit;

namespace AsmResolver.DotNet.Tests.Signatures
{
    public class DataBlobSignatureTest
    {
        [Fact]
        public void ReadingAndWritingGiveTheSameValueBoolean()
        {
            var constant = Constant.FromValue(Constants.Boolean);
            Assert.Equal(Constants.Boolean, constant.Value.InterpretData(constant.Type));
        }

        [Fact]
        public void ReadingAndWritingGiveTheSameValueChar()
        {
            var constant = Constant.FromValue(Constants.Char);
            Assert.Equal(Constants.Char, constant.Value.InterpretData(constant.Type));
        }

        [Fact]
        public void ReadingAndWritingGiveTheSameValueByte()
        {
            var constant = Constant.FromValue(Constants.Byte);
            Assert.Equal(Constants.Byte, constant.Value.InterpretData(constant.Type));
        }

        [Fact]
        public void ReadingAndWritingGiveTheSameValueSByte()
        {
            var constant = Constant.FromValue(Constants.SByte);
            Assert.Equal(Constants.SByte, constant.Value.InterpretData(constant.Type));
        }

        [Fact]
        public void ReadingAndWritingGiveTheSameValueUInt16()
        {
            var constant = Constant.FromValue(Constants.UInt16);
            Assert.Equal(Constants.UInt16, constant.Value.InterpretData(constant.Type));
        }

        [Fact]
        public void ReadingAndWritingGiveTheSameValueInt16()
        {
            var constant = Constant.FromValue(Constants.Int16);
            Assert.Equal(Constants.Int16, constant.Value.InterpretData(constant.Type));
        }

        [Fact]
        public void ReadingAndWritingGiveTheSameValueUInt32()
        {
            var constant = Constant.FromValue(Constants.UInt32);
            Assert.Equal(Constants.UInt32, constant.Value.InterpretData(constant.Type));
        }

        [Fact]
        public void ReadingAndWritingGiveTheSameValueInt32()
        {
            var constant = Constant.FromValue(Constants.Int32);
            Assert.Equal(Constants.Int32, constant.Value.InterpretData(constant.Type));
        }

        [Fact]
        public void ReadingAndWritingGiveTheSameValueUInt64()
        {
            var constant = Constant.FromValue(Constants.UInt64);
            Assert.Equal(Constants.UInt64, constant.Value.InterpretData(constant.Type));
        }

        [Fact]
        public void ReadingAndWritingGiveTheSameValueInt64()
        {
            var constant = Constant.FromValue(Constants.Int64);
            Assert.Equal(Constants.Int64, constant.Value.InterpretData(constant.Type));
        }

        [Fact]
        public void ReadingAndWritingGiveTheSameValueSingle()
        {
            var constant = Constant.FromValue(Constants.Single);
            Assert.Equal(Constants.Single, constant.Value.InterpretData(constant.Type));
        }

        [Fact]
        public void ReadingAndWritingGiveTheSameValueDouble()
        {
            var constant = Constant.FromValue(Constants.Double);
            Assert.Equal(Constants.Double, constant.Value.InterpretData(constant.Type));
        }

        [Fact]
        public void ReadingAndWritingGiveTheSameValueString()
        {
            var constant = Constant.FromValue(Constants.String);
            Assert.Equal(Constants.String, constant.Value.InterpretData(constant.Type));
        }
    }
}
