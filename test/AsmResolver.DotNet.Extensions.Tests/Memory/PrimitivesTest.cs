using System;
using AsmResolver.DotNet.Extensions.Memory;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Xunit;

namespace AsmResolver.DotNet.Extensions.Tests.Memory
{
    public sealed class PrimitivesTest : IClassFixture<TemporaryModuleFixture>
    {
        private readonly TemporaryModuleFixture _fixture;

        public PrimitivesTest(TemporaryModuleFixture fixture)
        {
            _fixture = fixture;
        }

        [Theory]
        [InlineData(ElementType.I1, 1u)]
        [InlineData(ElementType.U1, 1u)]
        [InlineData(ElementType.Boolean, 1u)]
        [InlineData(ElementType.I2, 2u)]
        [InlineData(ElementType.U2, 2u)]
        [InlineData(ElementType.Char, 2u)]
        [InlineData(ElementType.I4, 4u)]
        [InlineData(ElementType.U4, 4u)]
        [InlineData(ElementType.I8, 8u)]
        [InlineData(ElementType.U8, 8u)]
        [InlineData(ElementType.R4, 4u)]
        [InlineData(ElementType.R8, 8u)]
        public void CorlibSignature(ElementType type, uint expected)
        {
            var signature = _fixture.Module.CorLibTypeFactory.FromElementType(type);
            Assert.Equal(expected, signature.GetImpliedMemoryLayout(false).Size); // Bitness doesn't matter here
        }

        [Theory]
        [InlineData(ElementType.I)]
        [InlineData(ElementType.U)]
        [InlineData(ElementType.String)]
        [InlineData(ElementType.Object)]
        public void PointerSizedSignatures(ElementType type)
        {
            var signature = _fixture.Module.CorLibTypeFactory.FromElementType(type);
            Assert.Equal((uint) IntPtr.Size, signature.GetImpliedMemoryLayout(IntPtr.Size == 4).Size);
        }
    }
}