using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Xunit;

namespace AsmResolver.DotNet.Tests.Analysis
{
    public struct CustomStruct
    {
        public int Dummy1;

        public int Dummy2;
    }

    public struct CustomGenericStruct<T>
    {
        public T Dummy1;

        public ulong Dummy2;
    }

    [StructLayout(LayoutKind.Sequential, Size = 8)]
    public struct CustomStructWithBigSize
    {
        public int Dummy1;
    }
    
    [StructLayout(LayoutKind.Sequential, Size = 2)]
    public struct CustomStructWithSmallSize
    {
        public int Dummy1;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct CustomUnionStruct
    {
        [FieldOffset(0)]
        public long Dummy1;

        [FieldOffset(0)]
        public int Dummy2;

        [FieldOffset(8)]
        public float Dummy3;
    }

    public class SizeCalculatorTest
    {
        [Fact]
        public void CustomStruct()
        {
            var module = ModuleDefinition.FromFile(typeof(SizeCalculatorTest).Assembly.Location);
            var custom = (TypeDefinition) module.LookupMember(typeof(CustomStruct).MetadataToken);
            
            Assert.Equal(Unsafe.SizeOf<CustomStruct>(), custom.CalculateSize(IntPtr.Size == 4));
        }

        [Fact]
        public void CustomGenericStruct()
        {
            var module = ModuleDefinition.FromFile(typeof(SizeCalculatorTest).Assembly.Location);
            var custom = (TypeDefinition) module.LookupMember(typeof(CustomGenericStruct<byte>).MetadataToken);
            
            Assert.Equal(Unsafe.SizeOf<CustomGenericStruct<byte>>(), custom.CalculateSize(IntPtr.Size == 4));
        }

        [Fact]
        public void CustomStructWithBigSize()
        {
            var module = ModuleDefinition.FromFile(typeof(SizeCalculatorTest).Assembly.Location);
            var custom = (TypeDefinition) module.LookupMember(typeof(CustomStructWithBigSize).MetadataToken);
            
            Assert.Equal(Unsafe.SizeOf<CustomStructWithBigSize>(), custom.CalculateSize(IntPtr.Size == 4));
        }

        [Fact]
        public void CustomStructWithSmallSize()
        {
            var module = ModuleDefinition.FromFile(typeof(SizeCalculatorTest).Assembly.Location);
            var custom = (TypeDefinition) module.LookupMember(typeof(CustomStructWithSmallSize).MetadataToken);
            
            Assert.Equal(Unsafe.SizeOf<CustomStructWithSmallSize>(), custom.CalculateSize(IntPtr.Size == 4));
        }

        [Fact]
        public void CustomUnionStruct()
        {
            var module = ModuleDefinition.FromFile(typeof(SizeCalculatorTest).Assembly.Location);
            var custom = (TypeDefinition) module.LookupMember(typeof(CustomUnionStruct).MetadataToken);
            
            Assert.Equal(Unsafe.SizeOf<CustomUnionStruct>(), custom.CalculateSize(IntPtr.Size == 4));
        }
    }
}