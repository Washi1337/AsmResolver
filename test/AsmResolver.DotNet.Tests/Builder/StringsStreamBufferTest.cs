using System;
using System.Text;
using AsmResolver.DotNet.Builder;
using AsmResolver.DotNet.Builder.Metadata.Strings;
using AsmResolver.PE.DotNet.Metadata.Strings;
using Xunit;

namespace AsmResolver.DotNet.Tests.Builder
{
    public class StringsStreamBufferTest
    {
        [Fact]
        public void AddDistinct()
        {
            var buffer = new StringsStreamBuffer();

            const string string1 = "String 1";
            uint index1 = buffer.GetStringIndex(string1);

            const string string2 = "String 2";
            uint index2 = buffer.GetStringIndex(string2);

            Assert.NotEqual(index1, index2);

            var stringsStream = buffer.CreateStream();
            Assert.Equal(string1, stringsStream.GetStringByIndex(index1));
            Assert.Equal(string2, stringsStream.GetStringByIndex(index2));
        }

        [Fact]
        public void AddDuplicate()
        {
            var buffer = new StringsStreamBuffer();

            const string string1 = "String 1";
            uint index1 = buffer.GetStringIndex(string1);

            const string string2 = "String 1";
            uint index2 = buffer.GetStringIndex(string2);

            Assert.Equal(index1, index2);

            var stringsStream = buffer.CreateStream();
            Assert.Equal(string1, stringsStream.GetStringByIndex(index1));
        }

        [Fact]
        public void AddRaw()
        {
            var buffer = new StringsStreamBuffer();

            const string string1 = "String 1";
            var rawData = Encoding.UTF8.GetBytes(string1);

            uint index1 = buffer.AppendRawData(rawData);
            uint index2 = buffer.GetStringIndex(string1);

            Assert.NotEqual(index1, index2);

            var stringsStream = buffer.CreateStream();
            Assert.Equal(string1, stringsStream.GetStringByIndex(index2));
        }

        [Fact]
        public void AddStringWithZeroByte()
        {
            var buffer = new StringsStreamBuffer();
            Assert.Throws<ArgumentException>(() => buffer.GetStringIndex("Test\0Test"));
        }

        [Fact]
        public void ImportStringStreamShouldIndexExistingStrings()
        {
            var existingStringsStream = new SerializedStringsStream(StringsStream.DefaultName, Encoding.UTF8.GetBytes(
                "\0"
                + "String\0"
                + "LongerString\0"
                + "AnEvenLongerString\0"));

            var buffer = new StringsStreamBuffer();
            buffer.ImportStream(existingStringsStream);
            var newStream = buffer.CreateStream();

            Assert.Equal("String", newStream.GetStringByIndex(1));
            Assert.Equal("LongerString", newStream.GetStringByIndex(8));
            Assert.Equal("AnEvenLongerString", newStream.GetStringByIndex(21));
        }

        [Fact]
        public void ImportStringsStreamWithDuplicateStrings()
        {
            var existingStringsStream = new SerializedStringsStream(StringsStream.DefaultName, Encoding.UTF8.GetBytes(
                "\0"
                + "String\0"
                + "String\0"
                + "String\0"));

            var buffer = new StringsStreamBuffer();
            buffer.ImportStream(existingStringsStream);
            var newStream = buffer.CreateStream();

            Assert.Equal("String", newStream.GetStringByIndex(1));
            Assert.Equal("String", newStream.GetStringByIndex(8));
            Assert.Equal("String", newStream.GetStringByIndex(15));
        }

        [Fact]
        public void StringsStreamBufferShouldPreserveInvalidCharacters()
        {
            var str = new Utf8String(new byte[] {0x80, 0x79, 0x78 });

            var buffer = new StringsStreamBuffer();
            uint index = buffer.GetStringIndex(str);

            var stringsStream = buffer.CreateStream();
            Assert.Equal(str, stringsStream.GetStringByIndex(index));
        }

        [Fact]
        public void StringsStreamBufferShouldDistinguishDifferentInvalidCharacters()
        {
            var string1 = new Utf8String(new byte[] {0x80, 0x79, 0x78 });
            var string2 = new Utf8String(new byte[] {0x80, 0x79, 0x78 });
            var string3 = new Utf8String(new byte[] {0x81, 0x79, 0x78 });

            var buffer = new StringsStreamBuffer();
            uint index1 = buffer.GetStringIndex(string1);
            uint index2 = buffer.GetStringIndex(string2);
            uint index3 = buffer.GetStringIndex(string3);

            Assert.Equal(index1, index2);
            Assert.NotEqual(index1, index3);
        }
    }
}
