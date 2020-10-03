using System.Text;
using AsmResolver.DotNet.Builder;
using AsmResolver.DotNet.Builder.Metadata.UserStrings;
using AsmResolver.PE.DotNet.Metadata.UserStrings;
using Xunit;

namespace AsmResolver.DotNet.Tests.Builder
{
    public class UserStringsStreamBufferTest
    {
        [Fact]
        public void AddDistinct()
        {
            var buffer = new UserStringsStreamBuffer();

            const string string1 = "String 1";
            uint index1 = buffer.GetStringIndex(string1);

            const string string2 = "String 2";
            uint index2 = buffer.GetStringIndex(string2);

            Assert.NotEqual(index1, index2);
            
            var usStream = buffer.CreateStream();
            Assert.Equal(string1, usStream.GetStringByIndex(index1));
            Assert.Equal(string2, usStream.GetStringByIndex(index2));
        }

        [Fact]
        public void AddDuplicate()
        {
            var buffer = new UserStringsStreamBuffer();

            const string string1 = "String 1";
            uint index1 = buffer.GetStringIndex(string1);

            const string string2 = "String 1";
            uint index2 = buffer.GetStringIndex(string2);

            Assert.Equal(index1, index2);
            
            var usStream = buffer.CreateStream();
            Assert.Equal(string1, usStream.GetStringByIndex(index1));
        }

        [Fact]
        public void AddRaw()
        {
            var buffer = new UserStringsStreamBuffer();

            const string string1 = "String 1";
            var rawData = Encoding.UTF8.GetBytes(string1);

            uint index1 = buffer.AppendRawData(rawData);
            uint index2 = buffer.GetStringIndex(string1);

            Assert.NotEqual(index1, index2);

            var usStream = buffer.CreateStream();
            Assert.Equal(string1, usStream.GetStringByIndex(index2));
        }

        [Theory]
        [InlineData('\x00', 0)]
        [InlineData('\x01', 1)]
        [InlineData('\x08', 1)]
        [InlineData('\x09', 0)]
        [InlineData('\x0E', 1)]
        [InlineData('\x1F', 1)]
        [InlineData('\x26', 0)]
        [InlineData('\x27', 1)]
        [InlineData('\x2D', 1)]
        [InlineData('A', 0)]
        [InlineData('\x7F', 1)]
        [InlineData('\u3910', 1)]
        public void SpecialCharactersTerminatorByte(char specialChar, byte terminatorByte)
        {
            string value = "My String" + specialChar;
            
            var buffer = new UserStringsStreamBuffer();
            uint index = buffer.GetStringIndex(value);
            var usStream = buffer.CreateStream();

            Assert.Equal(value, usStream.GetStringByIndex(index));
            
            var reader = usStream.CreateReader();
            reader.Offset = (uint) (index + Encoding.Unicode.GetByteCount(value) + 1);
            byte b = reader.ReadByte();
            
            Assert.Equal(terminatorByte, b);
        }
        
        [Fact]
        public void ImportStreamShouldIndexExistingUserStrings()
        {
            var existingUserStringsStream = new SerializedUserStringsStream(UserStringsStream.DefaultName, new byte[]
            {
                0x00,

                // "User string"
                0x17,
                0x55, 0x00, 0x73, 0x00, 0x65, 0x00, 0x72, 0x00, 0x20, 0x00, 0x73, 0x00, 0x74, 0x00,
                0x72, 0x00, 0x69, 0x00, 0x6E, 0x00, 0x67, 0x00, 0x00,

                // "A longer user string"
                0x29,
                0x41, 0x00, 0x20, 0x00, 0x6C, 0x00, 0x6F, 0x00, 0x6E, 0x00, 0x67, 0x00, 0x65, 0x00,
                0x72, 0x00, 0x20, 0x00, 0x75, 0x00, 0x73, 0x00, 0x65, 0x00, 0x72, 0x00, 0x20, 0x00,
                0x73, 0x00, 0x74, 0x00, 0x72, 0x00, 0x69, 0x00, 0x6E, 0x00, 0x67, 0x00, 0x00,

                // "An even longer user string"
                0x35,
                0x41, 0x00, 0x6E, 0x00, 0x20, 0x00, 0x65, 0x00, 0x76, 0x00, 0x65, 0x00, 0x6E, 0x00,
                0x20, 0x00, 0x6C, 0x00, 0x6F, 0x00, 0x6E, 0x00, 0x67, 0x00, 0x65, 0x00, 0x72, 0x00,
                0x20, 0x00, 0x75, 0x00, 0x73, 0x00, 0x65, 0x00, 0x72, 0x00, 0x20, 0x00, 0x73, 0x00,
                0x74, 0x00, 0x72, 0x00, 0x69, 0x00, 0x6E, 0x00, 0x67, 0x00, 0x00, 0x00, 0x00, 0x00
            });
            
            var buffer = new UserStringsStreamBuffer();
            buffer.ImportStream(existingUserStringsStream);
            var newStream = buffer.CreateStream();

            Assert.Equal("User string", newStream.GetStringByIndex(1));
            Assert.Equal("A longer user string", newStream.GetStringByIndex(25));
            Assert.Equal("An even longer user string", newStream.GetStringByIndex(67));
        }
        
        [Fact]
        public void ImportStreamWithDuplicateUserStrings()
        { 
            var existingUserStringsStream = new SerializedUserStringsStream(UserStringsStream.DefaultName, new byte[]
            {
                0x00,

                // "User string"
                0x17,
                0x55, 0x00, 0x73, 0x00, 0x65, 0x00, 0x72, 0x00, 0x20, 0x00, 0x73, 0x00, 0x74, 0x00,
                0x72, 0x00, 0x69, 0x00, 0x6E, 0x00, 0x67, 0x00, 0x00,

                // "User string"
                0x17,
                0x55, 0x00, 0x73, 0x00, 0x65, 0x00, 0x72, 0x00, 0x20, 0x00, 0x73, 0x00, 0x74, 0x00,
                0x72, 0x00, 0x69, 0x00, 0x6E, 0x00, 0x67, 0x00, 0x00,

                // "User string"
                0x17,
                0x55, 0x00, 0x73, 0x00, 0x65, 0x00, 0x72, 0x00, 0x20, 0x00, 0x73, 0x00, 0x74, 0x00,
                0x72, 0x00, 0x69, 0x00, 0x6E, 0x00, 0x67, 0x00, 0x00,
            });
            
            var buffer = new UserStringsStreamBuffer();
            buffer.ImportStream(existingUserStringsStream);
            var newStream = buffer.CreateStream();

            Assert.Equal("User string", newStream.GetStringByIndex(1));
            Assert.Equal("User string", newStream.GetStringByIndex(25));
            Assert.Equal("User string", newStream.GetStringByIndex(49));
        }
        
        [Fact]
        public void ImportStreamWithGarbageData()
        {
            var existingUserStringsStream = new SerializedUserStringsStream(UserStringsStream.DefaultName, new byte[]
            {
                0x00,

                // "User string"
                0x17,
                0x55, 0x00, 0x73, 0x00, 0x65, 0x00, 0x72, 0x00, 0x20, 0x00, 0x73, 0x00, 0x74, 0x00,
                0x72, 0x00, 0x69, 0x00, 0x6E, 0x00, 0x67, 0x00, 0x00,

                0xAA,
                
                // "User string"
                0x17,
                0x55, 0x00, 0x73, 0x00, 0x65, 0x00, 0x72, 0x00, 0x20, 0x00, 0x73, 0x00, 0x74, 0x00,
                0x72, 0x00, 0x69, 0x00, 0x6E, 0x00, 0x67, 0x00, 0x00,
            });
            
            var buffer = new UserStringsStreamBuffer();
            buffer.ImportStream(existingUserStringsStream);
            var newStream = buffer.CreateStream();

            Assert.Equal("User string", newStream.GetStringByIndex(1));
            Assert.Equal("User string", newStream.GetStringByIndex(26));
        }
    }
}