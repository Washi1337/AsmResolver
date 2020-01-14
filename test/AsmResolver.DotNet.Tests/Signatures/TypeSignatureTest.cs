using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Xunit;

namespace AsmResolver.DotNet.Tests.Signatures
{
    public class TypeSignatureTest
    {
        [Theory]
        [InlineData(ElementType.Void)]
        [InlineData(ElementType.Boolean)]
        [InlineData(ElementType.Char)]
        [InlineData(ElementType.I1)]
        [InlineData(ElementType.U1)]
        [InlineData(ElementType.I2)]
        [InlineData(ElementType.U2)]
        [InlineData(ElementType.I4)]
        [InlineData(ElementType.U4)]
        [InlineData(ElementType.I8)]
        [InlineData(ElementType.U8)]
        [InlineData(ElementType.R4)]
        [InlineData(ElementType.R8)]
        [InlineData(ElementType.String)]
        [InlineData(ElementType.I)]
        [InlineData(ElementType.U)]
        [InlineData(ElementType.TypedByRef)]
        [InlineData(ElementType.Object)]
        public void ReadCorLibType(ElementType elementType)
        {
            var blob = new[] {(byte) elementType};
            var module = new ModuleDefinition("SomeModule");
            var typeSig = TypeSignature.FromReader(module, new ByteArrayReader(blob));
            Assert.Equal(elementType, typeSig.ElementType);
        }
    }
}