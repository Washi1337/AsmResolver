using System;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Xunit;

namespace AsmResolver.DotNet.Tests.Analysis
{
    public sealed class TypeMemoryLayoutTests
    {
        [Theory]
        [InlineData(ElementType.Boolean, sizeof(bool))]
        [InlineData(ElementType.Char, sizeof(char))]
        [InlineData(ElementType.I1, sizeof(sbyte))]
        [InlineData(ElementType.U1, sizeof(byte))]
        [InlineData(ElementType.I2, sizeof(short))]
        [InlineData(ElementType.U2, sizeof(ushort))]
        [InlineData(ElementType.I4, sizeof(int))]
        [InlineData(ElementType.U4, sizeof(uint))]
        [InlineData(ElementType.I8, sizeof(long))]
        [InlineData(ElementType.U8, sizeof(ulong))]
        public void Primitives(ElementType type, int realSize)
        {
            var module = ModuleDefinition.FromFile(typeof(TypeMemoryLayoutTests).Assembly.Location);
            var corlib = module.CorLibTypeFactory;
            var signature = corlib.FromElementType(type);

            var layout = signature.GetImpliedMemoryLayout(IntPtr.Size == 4);
            Assert.Equal(realSize, layout.Size);
        }

        [Theory]
        [InlineData(ElementType.String)]
        [InlineData(ElementType.TypedByRef)]
        [InlineData(ElementType.Object)]
        [InlineData(ElementType.I)]
        [InlineData(ElementType.U)]
        public void Native(ElementType type)
        {
            var module = ModuleDefinition.FromFile(typeof(TypeMemoryLayoutTests).Assembly.Location);
            var corlib = module.CorLibTypeFactory;
            var signature = corlib.FromElementType(type);

            var layout = signature.GetImpliedMemoryLayout(IntPtr.Size == 4);
            Assert.Equal(IntPtr.Size, layout.Size);
        }
    }
}