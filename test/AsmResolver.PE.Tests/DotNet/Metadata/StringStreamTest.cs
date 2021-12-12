using AsmResolver.PE.DotNet.Metadata.Strings;
using Xunit;

namespace AsmResolver.PE.Tests.DotNet.Metadata
{
    public class StringStreamTest
    {
        private static void AssertDoesNotHaveString(byte[] streamData, Utf8String? needle)
        {
            var stream = new SerializedStringsStream(streamData);
            Assert.False(stream.TryFindStringIndex(needle, out _));
        }

        private static void AssertHasString(byte[] streamData, Utf8String? needle)
        {
            var stream = new SerializedStringsStream(streamData);
            Assert.True(stream.TryFindStringIndex(needle, out uint actualIndex));
            Assert.Equal(needle, stream.GetStringByIndex(actualIndex));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("ABC")]
        [InlineData("DEF")]
        public void FindExistingString(string? value) => AssertHasString(new byte[]
            {
                0x00, 0x41, 0x42, 0x43, 0x00, 0x44, 0x45, 0x46, 0x00
            },
            value);

        [Theory]
        [InlineData("ABC")]
        [InlineData("DEF")]
        public void FindExistingStringButNotZeroTerminated(string value) => AssertDoesNotHaveString(new byte[]
            {
                0x00, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46
            },
            value);

        [Theory]
        [InlineData("CDE")]
        public void FindNonExistingString(string value) => AssertDoesNotHaveString(new byte[]
            {
                0x00, 0x41, 0x42, 0x43, 0x00, 0x44, 0x45, 0x46, 0x00
            },
            value);
    }
}
