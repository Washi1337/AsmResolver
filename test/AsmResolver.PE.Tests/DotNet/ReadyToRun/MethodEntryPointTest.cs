using AsmResolver.IO;
using AsmResolver.PE.DotNet.ReadyToRun;
using Xunit;

namespace AsmResolver.PE.Tests.DotNet.ReadyToRun
{
    public class MethodEntryPointTest
    {
        [Fact]
        public void NoFixups()
        {
            var entryPoint = new MethodEntryPoint(1337);

            entryPoint.UpdateOffsets(new RelocationParameters(0, 0));
            var reader = new BinaryStreamReader(entryPoint.WriteIntoArray());

            var newEntryPoint = MethodEntryPoint.FromReader(ref reader);
            Assert.Equal(1337u, newEntryPoint.RuntimeFunctionIndex);
            Assert.Empty(newEntryPoint.Fixups);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(1337, 0)]
        [InlineData(0, 1337)]
        [InlineData(1337, 1338)]
        public void SingleFixup(uint importIndex, uint slotIndex)
        {
            var entryPoint = new MethodEntryPoint(1234);
            entryPoint.Fixups.Add(new MethodFixup(importIndex, slotIndex));

            entryPoint.UpdateOffsets(new RelocationParameters(0, 0));
            var reader = new BinaryStreamReader(entryPoint.WriteIntoArray());

            var newEntryPoint = MethodEntryPoint.FromReader(ref reader);
            Assert.Equal(1234u, newEntryPoint.RuntimeFunctionIndex);
            Assert.Equal(entryPoint.Fixups, newEntryPoint.Fixups);
        }

        [Fact]
        public void MultipleFixups()
        {
            var entryPoint = new MethodEntryPoint(1234);
            entryPoint.Fixups.Add(new MethodFixup(1337, 1));
            entryPoint.Fixups.Add(new MethodFixup(1337, 2));
            entryPoint.Fixups.Add(new MethodFixup(1337, 3));
            entryPoint.Fixups.Add(new MethodFixup(1339, 11));
            entryPoint.Fixups.Add(new MethodFixup(1339, 12));
            entryPoint.Fixups.Add(new MethodFixup(1339, 13));

            entryPoint.UpdateOffsets(new RelocationParameters(0, 0));
            var reader = new BinaryStreamReader(entryPoint.WriteIntoArray());

            var newEntryPoint = MethodEntryPoint.FromReader(ref reader);
            Assert.Equal(1234u, newEntryPoint.RuntimeFunctionIndex);
            Assert.Equal(entryPoint.Fixups, newEntryPoint.Fixups);
        }
    }
}
