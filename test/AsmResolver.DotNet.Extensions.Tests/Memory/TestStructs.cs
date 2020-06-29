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
        public struct SingleFieldSequentialStruct
        {
            public int IntField;
        }
    }
}