using System.Runtime.InteropServices;
// ReSharper disable All

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


        [StructLayout(LayoutKind.Sequential, Size = 17)]
        public struct Struct1
        {
            public int Dummy1;
        }

        [StructLayout(LayoutKind.Sequential, Size = 23, Pack = 2)]
        public struct Struct2
        {
            public Struct1 Nest1;
        }

        [StructLayout(LayoutKind.Sequential, Size = 87, Pack = 64)]
        public struct Struct3
        {
            public Struct1 Nest1;

            public Struct2 Nest2;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Struct4
        {
            public Struct3 Nest1;

            public byte Dummy2;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct ExtremeStruct5
        {
            public Struct4 Nest1;

            public double Dummy1;
        }
    }
}
