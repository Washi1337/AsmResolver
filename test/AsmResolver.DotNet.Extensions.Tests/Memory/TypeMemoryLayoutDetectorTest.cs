using System;
using System.Runtime.CompilerServices;
using AsmResolver.DotNet.Memory;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Xunit;

namespace AsmResolver.DotNet.Tests.Memory
{
    public class TypeMemoryLayoutDetectorTest : IClassFixture<CurrentModuleFixture>
    {
        private readonly ModuleDefinition _module;

        public TypeMemoryLayoutDetectorTest(CurrentModuleFixture fixture)
        {
            _module = fixture.Module;
        }

        private TypeDefinition FindTestType(Type type)
        {
            return (TypeDefinition) _module.LookupMember(type.MetadataToken);
        }

        [Theory]
        [InlineData(ElementType.I1, sizeof(sbyte))]
        [InlineData(ElementType.I4, sizeof(int))]
        [InlineData(ElementType.I8, sizeof(long))]
        public void ValueTypedCorLibTypeShouldReturnElementSize(ElementType elementType, uint expectedSize)
        {
            var type = _module.CorLibTypeFactory.FromElementType(elementType);
            var layout = type.GetImpliedMemoryLayout(false);
            Assert.Equal(expectedSize, layout.Size);
        }

        [Theory]
        [InlineData(ElementType.String)]
        [InlineData(ElementType.Object)]
        [InlineData(ElementType.I)]
        [InlineData(ElementType.U)]
        public void ReferenceTypedCorLibTypeShouldReturnElementSize(ElementType elementType)
        {
            var type = _module.CorLibTypeFactory.FromElementType(elementType);
            var layout32 = type.GetImpliedMemoryLayout(true);
            Assert.Equal(4u, layout32.Size);
            var layout64 = type.GetImpliedMemoryLayout(false);
            Assert.Equal(8u, layout64.Size);
        }

        private void VerifySize<T>()
        {
            var type = FindTestType(typeof(T));
            var layout = type.GetImpliedMemoryLayout(false);
            Assert.Equal((uint) Unsafe.SizeOf<T>(), layout.Size);
        }

        [Fact]
        public void EmptyStruct() => VerifySize<TestStructs.EmptyStruct>();

        [Fact]
        public void SingleFieldSequentialStructDefaultPack() =>
            VerifySize<TestStructs.SingleFieldSequentialStructDefaultPack>();
        [Fact]
        public void SingleFieldSequentialStructPack1() =>
            VerifySize<TestStructs.SingleFieldSequentialStructPack1>();

        [Fact]
        public void MultipleFieldsSequentialStructDefaultPack() =>
            VerifySize<TestStructs.MultipleFieldsSequentialStructDefaultPack>();
        
        [Fact]
        public void MultipleFieldsSequentialStructPack1() =>
            VerifySize<TestStructs.MultipleFieldsSequentialStructPack1>();
        
        [Fact]
        public void LargeAndSmallFieldSequentialDefaultPack() =>
            VerifySize<TestStructs.LargeAndSmallFieldSequentialDefaultPack>();
        
        [Fact]
        public void NestedStruct1() => VerifySize<TestStructs.NestedStruct1>();
        
        [Fact]
        public void NestedStruct2() => VerifySize<TestStructs.NestedStruct2>();
        
        [Fact]
        public void NestedStructWithEnclosingPack1() => VerifySize<TestStructs.NestedStructWithEnclosingPack1>();
        
        [Fact]
        public void NestedStructWithNestedPack1() => VerifySize<TestStructs.NestedStructWithNestedPack1>();
        
        [Fact]
        public void NestedStructInNestedStruct() => VerifySize<TestStructs.NestedStructInNestedStruct>();
    }
}