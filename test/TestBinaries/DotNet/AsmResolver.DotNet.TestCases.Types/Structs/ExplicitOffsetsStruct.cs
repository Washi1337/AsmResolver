using System.Runtime.InteropServices;

namespace AsmResolver.DotNet.TestCases.Types.Structs
{
    [StructLayout(LayoutKind.Explicit)]
    public struct ExplicitOffsetsStruct
    {
        [FieldOffset(0)]
        public int IntField;
        
        [FieldOffset(10)]
        public byte ByteField;

        [FieldOffset(100)]
        public bool BoolField;
    }
}