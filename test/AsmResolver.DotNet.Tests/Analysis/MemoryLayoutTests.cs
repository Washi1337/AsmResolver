using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AsmResolver.DotNet.Analysis;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Xunit;

namespace AsmResolver.DotNet.Tests.Analysis
{
    public struct CustomStruct
    {
        public int Dummy1;

        public int Dummy2;
    }

    public struct CustomNest
    {
        public CustomGenericStruct<byte> Dummy1;

        public ulong Dummy2;
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

        public long Dummy2;
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

        [FieldOffset(12)]
        public CustomStructWithSmallSize Nest;
    }

    public class SizeCalculatorTest
    {
        [Fact]
        public void CustomStruct()
        {
            var module = ModuleDefinition.FromFile(typeof(SizeCalculatorTest).Assembly.Location);
            var custom = (TypeDefinition) module.LookupMember(typeof(CustomStruct).MetadataToken);
            
            Assert.Equal(Unsafe.SizeOf<CustomStruct>(), custom.GetImpliedMemoryLayout(IntPtr.Size == 4).Size);
        }

        [Fact]
        public void CustomGenericStruct()
        {
            var module = ModuleDefinition.FromFile(typeof(SizeCalculatorTest).Assembly.Location);
            var custom = (TypeDefinition) module.LookupMember(typeof(CustomNest).MetadataToken);
            
            Assert.Equal(Unsafe.SizeOf<CustomNest>(), custom.GetImpliedMemoryLayout(IntPtr.Size == 4).Size);
        }

        [Fact]
        public void CustomStructWithBigSize()
        {
            var module = ModuleDefinition.FromFile(typeof(SizeCalculatorTest).Assembly.Location);
            var custom = (TypeDefinition) module.LookupMember(typeof(CustomStructWithBigSize).MetadataToken);
            
            Assert.Equal(Unsafe.SizeOf<CustomStructWithBigSize>(), custom.GetImpliedMemoryLayout(IntPtr.Size == 4).Size);
        }

        [Fact]
        public void CustomStructWithSmallSize()
        {
            var module = ModuleDefinition.FromFile(typeof(SizeCalculatorTest).Assembly.Location);
            var custom = (TypeDefinition) module.LookupMember(typeof(CustomStructWithSmallSize).MetadataToken);
            
            Assert.Equal(Unsafe.SizeOf<CustomStructWithSmallSize>(), custom.GetImpliedMemoryLayout(IntPtr.Size == 4).Size);
        }

        [Fact]
        public void CustomUnionStruct()
        {
            var module = ModuleDefinition.FromFile(typeof(SizeCalculatorTest).Assembly.Location);
            var custom = (TypeDefinition) module.LookupMember(typeof(CustomUnionStruct).MetadataToken);
            
            Assert.Equal(Unsafe.SizeOf<CustomUnionStruct>(), custom.GetImpliedMemoryLayout(IntPtr.Size == 4).Size);
        }

        [Fact]
        public void ReferenceType()
        {
            var module = ModuleDefinition.FromFile(typeof(TypeMemoryLayout).Assembly.Location);
            var custom = (TypeDefinition) module.LookupMember(typeof(TypeMemoryLayout).MetadataToken);
            
            Assert.Equal(Unsafe.SizeOf<TypeMemoryLayout>(), custom.GetImpliedMemoryLayout(IntPtr.Size == 4).Size);
        }

        [Fact]
        public void PrimitiveInt32()
        {
            var module = ModuleDefinition.FromFile(typeof(SizeCalculatorTest).Assembly.Location);
            var custom = module.CorLibTypeFactory.Int32;
            
            Assert.Equal(Unsafe.SizeOf<int>(), custom.GetImpliedMemoryLayout(IntPtr.Size == 4).Size);
        }
    }
}