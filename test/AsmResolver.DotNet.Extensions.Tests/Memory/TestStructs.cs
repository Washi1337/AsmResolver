using System.Runtime.InteropServices;

// ReSharper disable All

namespace AsmResolver.DotNet.Tests.Memory
{
    public static class TestStructs
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct EmptyStruct
        {
            
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SingleFieldSequentialStructDefaultPack
        {
            public int IntField;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MultipleFieldsSequentialStructDefaultPack
        {
            public int IntField;
            public long LongField;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct LargeAndSmallFieldSequentialDefaultPack
        {
            public long LongField;
            public byte ByteField;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NestedStruct1
        {
            public SingleFieldSequentialStructDefaultPack Field1;
            public byte Field2;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NestedStruct2
        {
            public MultipleFieldsSequentialStructDefaultPack Field1;
            public byte Field2;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NestedStructInNestedStruct
        {
            public NestedStruct1 Field1;
            public byte Field2;
        }
    }
}