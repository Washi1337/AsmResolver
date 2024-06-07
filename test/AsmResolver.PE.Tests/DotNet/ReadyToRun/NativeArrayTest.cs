using System.Diagnostics;
using System.Linq;
using AsmResolver.IO;
using AsmResolver.PE.DotNet.ReadyToRun;
using Xunit;

namespace AsmResolver.PE.Tests.DotNet.ReadyToRun;

public class NativeArrayTest
{
    [DebuggerDisplay("{Value}")]
    private readonly struct IntValue : IWritable
    {
        public readonly int Value;

        public IntValue(int value) => Value = value;
        public uint GetPhysicalSize() => sizeof(uint);
        public void Write(BinaryStreamWriter writer) => writer.WriteInt32(Value);
        public static implicit operator IntValue(int value) => new(value);
        public static implicit operator int(IntValue value) => value.Value;
    }

    [Theory]
    [InlineData(new[] {1337})]
    [InlineData(new[] {1337, 1338})]
    [InlineData(new[] {1337, 1338, 1339})]
    [InlineData(new[] {1337, 1338, 1339, 1340})]
    [InlineData(new[] {1337, 1338, 1339, 1340, 1341})]
    [InlineData(new[] {
        1337, 1338, 1339, 1340,
        1341, 1342, 1343, 1344,
        1345, 1346, 1347, 1348,
        1349, 1350, 1351, 1352,
        1353, 1354 // count > 16 -> extra root.
    })]
    public void AddElements(int[] elements)
    {
        var array = new NativeArray<IntValue>();
        for (int i = 0; i < elements.Length; i++)
            array.Add(elements[i]);

        array.UpdateOffsets(new RelocationParameters(0, 0));
        byte[] raw = array.WriteIntoArray();
        var view = NativeArray<IntValue>.FromReader(new BinaryStreamReader(raw), reader => reader.ReadInt32());

        Assert.All(Enumerable.Range(0, elements.Length), i => Assert.Equal(elements[i], view[i].Value));
    }
}
