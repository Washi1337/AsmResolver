using System.Runtime.InteropServices;

namespace AsmResolver.DotNet.Tests.Memory
{
    public static class ExplicitTestStructs
    {
        [StructLayout(LayoutKind.Explicit)]
        public struct EmptyStruct
        {
        }
        
        [StructLayout(LayoutKind.Explicit)]
        public struct SingleFieldStructDefaultPackImplicitSize
        {
            [FieldOffset(0)]
            public int IntField;
        }
    }
}