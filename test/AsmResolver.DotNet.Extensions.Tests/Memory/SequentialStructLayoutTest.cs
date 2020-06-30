using System;
using System.Runtime.CompilerServices;
using AsmResolver.DotNet.Memory;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Xunit;

namespace AsmResolver.DotNet.Tests.Memory
{
    public class SequentialStructLayoutTest : StructLayoutTestBase
    {
        public SequentialStructLayoutTest(CurrentModuleFixture fixture)
            : base(fixture)
        {
        }

        [Theory]
        [InlineData(ElementType.I1, sizeof(sbyte))]
        [InlineData(ElementType.I4, sizeof(int))]
        [InlineData(ElementType.I8, sizeof(long))]
        public void ValueTypedCorLibTypeShouldReturnElementSize(ElementType elementType, uint expectedSize)
        {
            var type = Module.CorLibTypeFactory.FromElementType(elementType);
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
            var type = Module.CorLibTypeFactory.FromElementType(elementType);
            var layout32 = type.GetImpliedMemoryLayout(true);
            Assert.Equal(4u, layout32.Size);
            var layout64 = type.GetImpliedMemoryLayout(false);
            Assert.Equal(8u, layout64.Size);
        }

        [Fact]
        public void EmptyStruct() => VerifySize<SequentialTestStructs.EmptyStruct>();

        [Fact]
        public void SingleFieldSequentialStructDefaultPack() =>
            VerifySize<SequentialTestStructs.SingleFieldSequentialStructDefaultPack>();
        [Fact]
        public void SingleFieldSequentialStructPack1() =>
            VerifySize<SequentialTestStructs.SingleFieldSequentialStructPack1>();

        [Fact]
        public void MultipleFieldsSequentialStructDefaultPack() =>
            VerifySize<SequentialTestStructs.MultipleFieldsSequentialStructDefaultPack>();
        
        [Fact]
        public void MultipleFieldsSequentialStructPack1() =>
            VerifySize<SequentialTestStructs.MultipleFieldsSequentialStructPack1>();
        
        [Fact]
        public void LargeAndSmallFieldSequentialDefaultPack() =>
            VerifySize<SequentialTestStructs.LargeAndSmallFieldSequentialDefaultPack>();
        
        [Fact]
        public void NestedStruct1() => VerifySize<SequentialTestStructs.NestedStruct1>();
        
        [Fact]
        public void NestedStruct2() => VerifySize<SequentialTestStructs.NestedStruct2>();
        
        [Fact]
        public void NestedStructWithEnclosingPack1() => VerifySize<SequentialTestStructs.NestedStructWithEnclosingPack1>();
        
        [Fact]
        public void NestedStructWithNestedPack1() => VerifySize<SequentialTestStructs.NestedStructWithNestedPack1>();
        
        [Fact]
        public void NestedStructInNestedStruct() => VerifySize<SequentialTestStructs.NestedStructInNestedStruct>();
        
        [Fact]
        public void ThreeLevelsNestingSequentialStructDefaultPack() => 
            VerifySize<SequentialTestStructs.ThreeLevelsNestingSequentialStructDefaultPack>();
        
        [Fact]
        public void ThreeLevelsNestingSequentialStructPack1() => 
            VerifySize<SequentialTestStructs.ThreeLevelsNestingSequentialWithNestedStructPack1>();
        
        [Fact]
        public void ExplicitlySizedEmptyStruct() => VerifySize<SequentialTestStructs.ExplicitlySizedEmptyStruct>();
        
        [Fact]
        public void ExplicitlySizedSingleField() => VerifySize<SequentialTestStructs.ExplicitlySizedSingleField>();
        
        [Fact]
        public void ExplicitlySizedSmallerExplicitSizeThanActualSize() =>
            VerifySize<SequentialTestStructs.ExplicitlySizedSmallerExplicitSizeThanActualSize>();
        
        [Fact]
        public void StructWithPrimitiveFieldSmallerThanPack() =>
            VerifySize<SequentialTestStructs.StructWithPrimitiveFieldSmallerThanPack>();
        
        [Fact]
        public void StructWithStructFieldSmallerThanPack() =>
            VerifySize<SequentialTestStructs.StructWithStructFieldSmallerThanPack>();
    }
}