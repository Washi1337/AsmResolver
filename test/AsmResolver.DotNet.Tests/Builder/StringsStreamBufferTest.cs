using System;
using System.Collections.Generic;
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

        [Fact]
        public void OptimizeEmpty()
        {
            var buffer = new StringsStreamBuffer();
            buffer.Optimize();

            Assert.True(buffer.IsEmpty);
        }

        private void OptimizeAndVerifyIndices(StringsStreamBuffer buffer, params string[] values)
        {
            uint[] indices = new uint[values.Length];
            for (int i = 0; i < values.Length; i++)
                indices[i] = buffer.GetStringIndex(values[i]);
            var translationTable = buffer.Optimize();
            for (int i = 0; i < values.Length; i++)
                Assert.Equal(translationTable[indices[i]], buffer.GetStringIndex(values[i]));
        }

        [Fact]
        public void AddStringsWithSameSuffixShouldResultInStringReuseAfterOptimize()
        {
            var buffer = new StringsStreamBuffer();
            OptimizeAndVerifyIndices(buffer,
                "AAAABBBB",
                "BBBB",
                "BB");

            uint index1 = buffer.GetStringIndex("AAAABBBB");
            uint index2 = buffer.GetStringIndex("BBBB");
            uint index3 = buffer.GetStringIndex("BB");

            Assert.Equal(index1 + 4u, index2);
            Assert.Equal(index1 + 6u, index3);

            var stream = buffer.CreateStream();
            Assert.Equal((1u + 8u + 1u).Align(4), stream.GetPhysicalSize());
        }

        [Fact]
        public void AddStringsWithSameSuffixReversedShouldResultInStringReuseAfterOptimize()
        {
            var buffer = new StringsStreamBuffer();
            OptimizeAndVerifyIndices(buffer,
                "BB",
                "BBBB",
                "AAAABBBB");

            uint index1 = buffer.GetStringIndex("AAAABBBB");
            uint index2 = buffer.GetStringIndex("BBBB");
            uint index3 = buffer.GetStringIndex("BB");

            Assert.Equal(index1 + 4u, index2);
            Assert.Equal(index1 + 6u, index3);

            var stream = buffer.CreateStream();
            Assert.Equal((1u + 8u + 1u).Align(4), stream.GetPhysicalSize());
        }

        [Fact]
        public void OptimizeAfterImportedStreamShouldPreserveImportedData()
        {
            var existingStringsStream = new SerializedStringsStream(StringsStream.DefaultName, Encoding.UTF8.GetBytes(
                "\0"
                + "AAAABBBB\0"
                + "abc\0"));

            var buffer = new StringsStreamBuffer();
            buffer.ImportStream(existingStringsStream);
            OptimizeAndVerifyIndices(buffer, "AAAABBBB", "BBBB");

            Assert.Equal(1u, buffer.GetStringIndex("AAAABBBB"));
            Assert.Equal(1u + 4u, buffer.GetStringIndex("BBBB"));

            var stream = buffer.CreateStream();
            Assert.Equal((1u + 9u + 4u).Align(4), stream.GetPhysicalSize());
        }

        [Fact]
        public void OptimizeAfterImportedStreamShouldPreserveImportedData2()
        {
            var existingStringsStream = new SerializedStringsStream(StringsStream.DefaultName, Encoding.UTF8.GetBytes(
                "\0"
                + "AAAABBBB\0"
                + "abc\0"));

            var buffer = new StringsStreamBuffer();
            buffer.ImportStream(existingStringsStream);
            OptimizeAndVerifyIndices(buffer, "AAAABBBBCCCC");

            var stream = buffer.CreateStream();
            Assert.Equal("AAAABBBB", stream.GetStringByIndex(1u));
            Assert.Equal("AAAABBBBCCCC", stream.GetStringByIndex(1u + 9u + 4u));
        }

        [Fact]
        public void OptimizeLongChainOfSuffixStrings()
        {
            const string templateString = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            var oldIndices = new List<uint>();

            var buffer = new StringsStreamBuffer();
            for (int i = 0; i < templateString.Length; i++)
                oldIndices.Add(buffer.GetStringIndex(templateString[i..]));
            var table = buffer.Optimize();

            for (uint i = 0; i < templateString.Length; i++)
            {
                Assert.Equal(i + 1, buffer.GetStringIndex(templateString[(int) i..]));
                Assert.Equal(i + 1, table[oldIndices[(int) i]]);
            }

            var stream = buffer.CreateStream();
            Assert.Equal((1u + (uint)templateString.Length + 1u).Align(4), stream.GetPhysicalSize());
        }

        [Fact]
        public void OptimizeMultipleLongChainsOfSuffixStrings()
        {
            const string templateString1 = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            const string templateString2 = "zyxwvutsrqponmlkjihgfedcbaZYXWVUTSRQPONMLKJIHGFEDCBA";

            var strings = new List<string>();
            for (int i = 0; i < templateString1.Length; i++)
                strings.Add(templateString1[i..]);
            for (int i = 0; i < templateString2.Length; i++)
                strings.Add(templateString2[i..]);
            strings.Sort((_, _) => Guid.NewGuid().CompareTo(Guid.NewGuid()));

            var buffer = new StringsStreamBuffer();
            foreach (string s in strings)
                buffer.GetStringIndex(s);
            buffer.Optimize();

            var stream = buffer.CreateStream();
            Assert.Equal(
                (1u + (uint)templateString1.Length + 1u + (uint)templateString2.Length + 1u).Align(4),
                stream.GetPhysicalSize());
        }

    }
}
