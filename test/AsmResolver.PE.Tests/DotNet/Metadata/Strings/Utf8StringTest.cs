using System;
using System.Linq;
using AsmResolver.PE.DotNet.Metadata.Strings;
using Xunit;

namespace AsmResolver.PE.Tests.DotNet.Metadata.Strings
{
    public class Utf8StringTest
    {
        [Fact]
        public void ConvertFromString()
        {
            const string text = "Hello, World";
            Utf8String utf8 = text;
            Assert.Equal(text, utf8.Value);
        }

        [Theory]
        [InlineData("ABC", "ABC")]
        [InlineData("ABC", "DEF")]
        public void StringEquality(string a, string b)
        {
            var s1 = new Utf8String(a);
            var s2 = new Utf8String(b);
            Assert.Equal(a == b, s1 == s2);
        }

        [Theory]
        [InlineData(new byte[] { 1, 2, 3 }, new byte[] { 1, 2, 3 })]
        [InlineData(new byte[] { 1, 2, 3 }, new byte[] { 4, 5, 6 })]
        public void ByteEquality(byte[] a, byte[] b)
        {
            var s1 = new Utf8String(a);
            var s2 = new Utf8String(b);
            Assert.Equal(ByteArrayEqualityComparer.Instance.Equals(a, b), s1 == s2);
        }

        [Theory]
        [InlineData("")]
        [InlineData("Hello")]
        public void StringNullOrEmpty(string x)
        {
            var s1 = new Utf8String(x);
            Assert.Equal(string.IsNullOrEmpty(x), Utf8String.IsNullOrEmpty(s1));
        }

        [Theory]
        [InlineData("Hello, ", "World")]
        [InlineData("", "World")]
        [InlineData("Hello", "")]
        [InlineData("", null)]
        [InlineData(null, "")]
        [InlineData(null, null)]
        [InlineData("", "")]
        public void StringConcat(string? a, string? b)
        {
            Utf8String? s1 = a;
            Utf8String? s2 = b;
            Assert.Equal(a + b, s1 + s2);
        }

        [Theory]
        [InlineData(new byte[] { 0x41, 0x42 }, new byte[] { 0x43, 0x44 })]
        [InlineData(new byte[] { 0x41, 0x42 }, new byte[0])]
        [InlineData(new byte[0], new byte[] { 0x43, 0x44 })]
        [InlineData(new byte[0], null)]
        [InlineData(null, new byte[0])]
        [InlineData(null, null)]
        [InlineData(new byte[0], new byte[0])]
        public void ByteConcat(byte[]? a, byte[]? b)
        {
            Utf8String? s1 = a;
            Utf8String? s2 = b;
            Assert.Equal((a ?? Array.Empty<byte>()).Concat(b ?? Array.Empty<byte>()), (s1 + s2)?.GetBytes());
        }
    }
}
