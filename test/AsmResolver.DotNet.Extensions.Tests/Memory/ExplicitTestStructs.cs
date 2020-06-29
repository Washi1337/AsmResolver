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
        
        [StructLayout(LayoutKind.Explicit)]
        public struct MultipleFieldsStructDefaultPackImplicitSize
        {
            [FieldOffset(0)]
            public int IntField;
            [FieldOffset(100)]
            public long LongField;
        }
        
        [StructLayout(LayoutKind.Explicit)]
        public struct OverlappingFieldsStructDefaultPackImplicitSize
        {
            [FieldOffset(0)]
            public int IntField;
            [FieldOffset(0)]
            public long LongField;
        }
        
        [StructLayout(LayoutKind.Explicit, Size = 100)]
        public struct OverlappingFieldsStructDefaultPackExplicitSize
        {
            [FieldOffset(0)]
            public int IntField;
            [FieldOffset(0)]
            public long LongField;
        }
        
        [StructLayout(LayoutKind.Explicit)]
        public struct NestedStruct
        {
            [FieldOffset(0)]
            public OverlappingFieldsStructDefaultPackImplicitSize Field1;
            
            [FieldOffset(100)]
            public byte Field2;
        }
        
        [StructLayout(LayoutKind.Explicit)]
        public struct NestedStructOverlapping
        {
            [FieldOffset(0)]
            public OverlappingFieldsStructDefaultPackImplicitSize Field1;
            
            [FieldOffset(0)]
            public byte Field2;
        }
        
        [StructLayout(LayoutKind.Explicit)]
        public struct NestedStructInNestedStructOverlapping
        {
            [FieldOffset(0)]
            public NestedStructOverlapping Field1;
            
            [FieldOffset(0)]
            public byte Field2;
        }
    }
}