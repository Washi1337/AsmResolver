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
    }
}