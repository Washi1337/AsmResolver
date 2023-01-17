using System;
using System.Linq;
using AsmResolver.Patching;
using Xunit;

namespace AsmResolver.Tests.Patching
{
    public class PatchedSegmentTest
    {
        private readonly DataSegment _input = new(Enumerable
            .Range(0, 1000)
            .Select(x => (byte) (x & 0xFF))
            .ToArray());

        [Fact]
        public void SimpleBytesPatch()
        {
            var patched = new PatchedSegment(_input);

            uint relativeOffset = 10;
            byte[] newData = {0xFF, 0xFE, 0xFD, 0xFC};
            patched.Patches.Add(new BytesPatch(relativeOffset, newData));

            byte[] expected = _input.ToArray();
            Buffer.BlockCopy(newData, 0, expected, (int) relativeOffset, newData.Length);

            byte[] result = patched.WriteIntoArray();
            Assert.Equal(expected, result);
        }
    }
}
