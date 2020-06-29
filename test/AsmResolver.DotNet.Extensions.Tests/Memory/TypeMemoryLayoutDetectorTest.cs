using System;
using AsmResolver.DotNet.Memory;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Xunit;

namespace AsmResolver.DotNet.Tests.Memory
{
    public class TypeMemoryLayoutDetectorTest : IClassFixture<CurrentModuleFixture>
    {
        private readonly ModuleDefinition _module;
        private readonly TypeMemoryLayoutDetector _detector;
        
        public TypeMemoryLayoutDetectorTest(CurrentModuleFixture fixture)
        {
            _module = fixture.Module;
            _detector = new TypeMemoryLayoutDetector(new GenericContext(), IntPtr.Size == 4);
        }
        
        [Theory]
        [InlineData(ElementType.I1, sizeof(sbyte))]
        [InlineData(ElementType.I4, sizeof(int))]
        [InlineData(ElementType.I8, sizeof(long))]
        public void ValueTypedCorLibTypeShouldReturnElementSize(ElementType elementType, uint expectedSize)
        {
            var type = _module.CorLibTypeFactory.FromElementType(elementType);
            var layout = type.AcceptVisitor(_detector);
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
            var layout = type.AcceptVisitor(_detector);
            Assert.Equal((uint) IntPtr.Size, layout.Size);
        }
    }
}