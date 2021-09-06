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
        [InlineData(new byte[] { 0x41, 0x42, 0x43 }, 3)]
        [InlineData(new byte[] { 0x80, 0x42, 0x43 }, 3)]
        public void LengthBeforeAndAfterValueAccessShouldBeConsistentProperty(byte[] data, int expected)
        {
            var s1 = new Utf8String(data);
            Assert.Equal(expected, s1.Length);
            _ = s1.Value;
            Assert.Equal(expected, s1.Length);
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

        [Theory]
        [InlineData("0123456789", '0', 0)]
        [InlineData("0123456789", '5', 5)]
        [InlineData("0123456789", 'a', -1)]
        public void IndexOfChar(string haystack, char needle, int expected)
        {
            Assert.Equal(expected, new Utf8String(haystack).IndexOf(needle));
        }

        [Theory]
        [InlineData("012345678901234567890123456789", '0', 0, 0)]
        [InlineData("012345678901234567890123456789", '0', 1, 10)]
        [InlineData("012345678901234567890123456789", '0', 11, 20)]
        [InlineData("012345678901234567890123456789", '0', 21, -1)]
        public void IndexOfCharStartingAtIndex(string haystack, char needle, int startIndex, int expected)
        {
            Assert.Equal(expected, new Utf8String(haystack).IndexOf(needle, startIndex));
        }

        [Theory]
        [InlineData("01234567890123456789", '0', 10)]
        [InlineData("01234567890123456789", '5', 15)]
        [InlineData("01234567890123456789", 'a', -1)]
        public void LastIndexOfChar(string haystack, char needle, int expected)
        {
            Assert.Equal(expected, new Utf8String(haystack).LastIndexOf(needle));
        }

        [Theory]
        [InlineData("012345678901234567890123456789", '0', 29, 20)]
        [InlineData("012345678901234567890123456789", '0', 19, 10)]
        [InlineData("012345678901234567890123456789", '0', 9, 0)]
        [InlineData("012345678901234567890123456789", 'a', 20, -1)]
        public void LastIndexOfCharStartingAtIndex(string haystack, char needle, int startIndex, int expected)
        {
            Assert.Equal(expected, new Utf8String(haystack).LastIndexOf(needle, startIndex));
        }

        [Theory]
        [InlineData("0123456789", "01", 0)]
        [InlineData("0123456789", "56", 5)]
        [InlineData("0123456789", "ab", -1)]
        public void IndexOfString(string haystack, string needle, int expected)
        {
            Assert.Equal(expected, new Utf8String(haystack).IndexOf(needle));
        }

        [Theory]
        [InlineData("012345678901234567890123456789", "01", 0, 0)]
        [InlineData("012345678901234567890123456789", "01", 1, 10)]
        [InlineData("012345678901234567890123456789", "01", 11, 20)]
        [InlineData("012345678901234567890123456789", "01", 21, -1)]
        public void IndexOfStringStartingAtIndex(string haystack, string needle, int startIndex, int expected)
        {
            Assert.Equal(expected, new Utf8String(haystack).IndexOf(needle, startIndex));
        }
    }
}
