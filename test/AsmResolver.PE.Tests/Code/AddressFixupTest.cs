using System;
using System.Linq;
using AsmResolver.Patching;
using AsmResolver.PE.Code;
using Xunit;

namespace AsmResolver.PE.Tests.Code
{
    public class AddressFixupTest
    {
        private readonly DataSegment _input = new(Enumerable
            .Range(0, 1000)
            .Select(x => (byte) (x & 0xFF))
            .ToArray());

        private readonly ISymbol _dummySymbol = new Symbol(new VirtualAddress(0x0000_2000));

        [Fact]
        public void PatchAbsolute32BitAddress()
        {
            var patched = new PatchedSegment(_input);
            patched.Patches.Add(new AddressFixupPatch(
                new AddressFixup(10, AddressFixupType.Absolute32BitAddress, _dummySymbol)));

            patched.UpdateOffsets(new RelocationParameters(0x0040_0000, 0, 0, true));

            byte[] expected = _input.ToArray();
            Buffer.BlockCopy(BitConverter.GetBytes(0x0040_2000), 0, expected, 10, sizeof(int));

            byte[] actual = patched.WriteIntoArray();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void PatchAbsolute32BitAddressFluent()
        {
            var patched = _input
                .AsPatchedSegment()
                .Patch(10, AddressFixupType.Absolute32BitAddress, _dummySymbol);

            patched.UpdateOffsets(new RelocationParameters(0x0040_0000, 0, 0, true));

            byte[] expected = _input.ToArray();
            Buffer.BlockCopy(BitConverter.GetBytes(0x0040_2000), 0, expected, 10, sizeof(int));

            byte[] actual = patched.WriteIntoArray();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void PatchAbsolute64BitAddress()
        {
            var patched = new PatchedSegment(_input);
            patched.Patches.Add(new AddressFixupPatch(
                new AddressFixup(10, AddressFixupType.Absolute64BitAddress, _dummySymbol)));

            patched.UpdateOffsets(new RelocationParameters(0x0000_0001_0000_0000, 0, 0, true));

            byte[] expected = _input.ToArray();
            Buffer.BlockCopy(BitConverter.GetBytes(0x0000_0001_0000_2000), 0, expected, 10, sizeof(ulong));

            byte[] actual = patched.WriteIntoArray();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void PatchAbsolute64BitAddressFluent()
        {
            var patched = _input
                .AsPatchedSegment()
                .Patch(10, AddressFixupType.Absolute64BitAddress, _dummySymbol);

            patched.UpdateOffsets(new RelocationParameters(0x0000_0001_0000_0000, 0, 0, true));

            byte[] expected = _input.ToArray();
            Buffer.BlockCopy(BitConverter.GetBytes(0x0000_0001_0000_2000), 0, expected, 10, sizeof(ulong));

            byte[] actual = patched.WriteIntoArray();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void PatchRelative32BitAddress()
        {
            var patched = new PatchedSegment(_input);
            patched.Patches.Add(new AddressFixupPatch(
                new AddressFixup(10, AddressFixupType.Relative32BitAddress, _dummySymbol)));

            patched.UpdateOffsets(new RelocationParameters(0x0040_0000, 0, 0, true));

            byte[] expected = _input.ToArray();
            Buffer.BlockCopy(BitConverter.GetBytes(0x2000 - 10 - 4), 0, expected, 10, sizeof(int));

            byte[] actual = patched.WriteIntoArray();
            Assert.Equal(expected, actual);
        }
    }
}
