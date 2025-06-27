using System;
using System.IO;
using System.Linq;
using AsmResolver.IO;
using AsmResolver.Patching;
using Xunit;

namespace AsmResolver.Tests.Patching
{
    public class PatchedSegmentTest
    {
        [Theory]
        [InlineData(0x0u, 0u)]
        [InlineData(0x1000u, 0u)]
        [InlineData(0x0u, 0x1000u)]
        [InlineData(0x1000u, 0x1000u)]
        public void BytesPatch(uint segmentOffset, uint writerOffset)
        {
            const uint relativeOffset = 10;
            byte[] newData = [0xFF, 0xFE, 0xFD, 0xFC];

            var input = new DataSegment(
                Enumerable.Range(0, 1000)
                    .Select(x => (byte) (x & 0xFF))
                    .ToArray()
            );
            input.UpdateOffsets(new RelocationParameters(segmentOffset, segmentOffset));

            // Prepare patch
            var patched = new PatchedSegment(input);
            patched.Patches.Add(new BytesPatch(relativeOffset, newData));

            // Construct expected buffer
            var stream = new MemoryStream();
            var writer = new BinaryStreamWriter(stream);
            writer.WriteZeroes((int) writerOffset);
            input.Write(writer);
            writer.Offset = writerOffset + relativeOffset;
            writer.WriteBytes(newData);
            byte[] expected = stream.ToArray();

            // Construct actual buffer
            stream = new MemoryStream();
            writer = new BinaryStreamWriter(stream);
            writer.WriteZeroes((int) writerOffset);
            patched.Write(writer);
            byte[] actual = stream.ToArray();

            // Verify
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(0x0u, 0u)]
        [InlineData(0x1000u, 0u)]
        [InlineData(0x0u, 0x1000u)]
        [InlineData(0x1000u, 0x1000u)]
        public void SegmentPatch(uint segmentOffset, uint writerOffset)
        {
            const uint relativeOffset = 10;
            byte[] newData = [0xFF, 0xFE, 0xFD, 0xFC];

            var input = new DataSegment(
                Enumerable.Range(0, 1000)
                    .Select(x => (byte) (x & 0xFF))
                    .ToArray()
            );
            input.UpdateOffsets(new RelocationParameters(segmentOffset, segmentOffset));

            // Prepare patch
            var patched = new PatchedSegment(input);
            patched.Patches.Add(new SegmentPatch(relativeOffset, new DataSegment(newData)));

            // Construct expected buffer
            var stream = new MemoryStream();
            var writer = new BinaryStreamWriter(stream);
            writer.WriteZeroes((int) writerOffset);
            input.Write(writer);
            writer.Offset = writerOffset + relativeOffset;
            writer.WriteBytes(newData);
            byte[] expected = stream.ToArray();

            // Construct actual buffer
            stream = new MemoryStream();
            writer = new BinaryStreamWriter(stream);
            writer.WriteZeroes((int) writerOffset);
            patched.Write(writer);
            byte[] actual = stream.ToArray();

            // Verify
            Assert.Equal(expected, actual);
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
