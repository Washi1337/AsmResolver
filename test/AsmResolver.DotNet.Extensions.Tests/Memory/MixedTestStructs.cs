using System.Runtime.InteropServices;

namespace AsmResolver.DotNet.Tests.Memory
{
    public static class MixedTestStructs
    {
        [StructLayout(LayoutKind.Sequential, Size = 123, Pack = 1)]
        public struct ExplicitlySizedSequentialStruct
        {
            public int Field1;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct ExplicitStructWithSequentialStruct
        {
            [FieldOffset(2)]
            public ExplicitlySizedSequentialStruct Field1;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct ExplicitStructWithTwoSequentialStructs
        {
            [FieldOffset(2)]
            public ExplicitlySizedSequentialStruct Field1;
            
            [FieldOffset(10)]
            public ExplicitlySizedSequentialStruct Field2;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct ExtremeStruct1
        {
            [FieldOffset(2)]
            public ExplicitStructWithTwoSequentialStructs Field1;
            
            [FieldOffset(10)]
            public ExplicitStructWithTwoSequentialStructs Field2;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ExtremeStruct2
        {
            public ExplicitStructWithTwoSequentialStructs Field1;
            
            public ExplicitStructWithTwoSequentialStructs Field2;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 16)]
        public struct ExtremeStruct3
        {
            public ExplicitStructWithTwoSequentialStructs Field1;
            
            public ExplicitStructWithTwoSequentialStructs Field2;
        }
    }
}