using System.Runtime.InteropServices;

// ReSharper disable All

namespace AsmResolver.DotNet.Tests.Memory
{
    public static class SequentialTestStructs
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

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SingleFieldSequentialStructPack1
        {
            public int IntField;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MultipleFieldsSequentialStructDefaultPack
        {
            public int IntField;
            public long LongField;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct MultipleFieldsSequentialStructPack1
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

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct NestedStructWithEnclosingPack1
        {
            public MultipleFieldsSequentialStructDefaultPack Field1;
            public byte Field2;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct NestedStructWithNestedPack1
        {
            public MultipleFieldsSequentialStructPack1 Field1;
            public byte Field2;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NestedStructInNestedStruct
        {
            public NestedStruct1 Field1;
            public byte Field2;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ThreeLevelsNestingSequentialStructDefaultPack
        {
            public NestedStructInNestedStruct Field1;
            public byte Field2;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ThreeLevelsNestingSequentialWithNestedStructPack1
        {
            public NestedStructWithEnclosingPack1 Field1;
            public byte Field2;
        }

        [StructLayout(LayoutKind.Sequential, Size = 400)]
        public struct ExplicitlySizedEmptyStruct
        {
        }

        [StructLayout(LayoutKind.Sequential, Size = 400)]
        public struct ExplicitlySizedSingleField
        {
            public int IntField;
        }

        [StructLayout(LayoutKind.Sequential, Size = 2)]
        public struct ExplicitlySizedSmallerExplicitSizeThanActualSize
        {
            public int IntField;
        }
    }
}