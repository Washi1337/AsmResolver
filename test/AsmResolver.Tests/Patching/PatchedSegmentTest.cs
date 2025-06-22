using System;
using System.Linq;
using AsmResolver.Patching;
using Xunit;

namespace AsmResolver.Tests.Patching
{
    public class PatchedSegmentTest
    {
        [Theory]
        [InlineData(0x0u)]
        [InlineData(0x1000u)]
        public void SimpleBytesPatch(uint baseOffset)
        {
            var input = new DataSegment(
                Enumerable.Range(0, 1000)
                    .Select(x => (byte) (x & 0xFF))
                    .ToArray()
            );
            input.UpdateOffsets(new RelocationParameters(baseOffset, baseOffset));

            var patched = new PatchedSegment(input);

            const uint relativeOffset = 10;
            byte[] newData = [0xFF, 0xFE, 0xFD, 0xFC];
            patched.Patches.Add(new BytesPatch(relativeOffset, newData));

            byte[] expected = input.ToArray();
            Buffer.BlockCopy(newData, 0, expected, (int) relativeOffset, newData.Length);

            byte[] result = patched.WriteIntoArray();
            Assert.Equal(expected, result);
        }

        [Fact]
        public void DoublePatchedSegmentShouldReturnSameInstance()
        {
            var x = new DataSegment(
                Enumerable.Range(0, 1000)
                    .Select(x1 => (byte) (x1 & 0xFF))
                    .ToArray()
            ).AsPatchedSegment();

            var y = x.AsPatchedSegment();
            Assert.Same(x, y);
        }

        [Fact]
        public void SimpleBytesPatchFluent()
        {
            const uint relativeOffset = 10;
            byte[] newData = [0xFF, 0xFE, 0xFD, 0xFC];

            var input = new DataSegment(
                Enumerable.Range(0, 1000)
                    .Select(x => (byte) (x & 0xFF))
                    .ToArray()
            );

            var patched = input
                .AsPatchedSegment()
                .Patch(relativeOffset, newData);

            byte[] expected = input.ToArray();
            Buffer.BlockCopy(newData, 0, expected, (int) relativeOffset, newData.Length);

            byte[] result = patched.WriteIntoArray();
            Assert.Equal(expected, result);
        }
    }
}
