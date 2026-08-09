using AsmResolver.DotNet.Builder;
using AsmResolver.DotNet.Code.Cil;
using Xunit;

namespace AsmResolver.DotNet.Tests.Code.Cil
{
    public class CilOperandBuilderTest
    {
        [Fact]
        public void MaximumUserStringIndexProducesValidToken()
        {
            var diagnostics = new DiagnosticBag();
            var builder = new CilOperandBuilder(new UserStringTokenProvider(0x00FFFFFF), diagnostics);

            Assert.Equal(0x70FFFFFFu, builder.GetStringToken("value"));
            Assert.False(diagnostics.HasErrors);
        }

        [Fact]
        public void OversizedUserStringIndexProducesDiagnostic()
        {
            var diagnostics = new DiagnosticBag();
            var builder = new CilOperandBuilder(new UserStringTokenProvider(0x01000000), diagnostics);

            Assert.Equal(0u, builder.GetStringToken("value"));
            var exception = Assert.IsType<MetadataBuilderException>(Assert.Single(diagnostics.Exceptions));
            Assert.Contains("does not fit in a 24-bit metadata token", exception.Message);
            Assert.False(diagnostics.IsFatal);
        }

        private sealed class UserStringTokenProvider : OriginalMetadataTokenProvider, IMetadataTokenProvider
        {
            private readonly uint _index;

            public UserStringTokenProvider(uint index)
                : base(null)
            {
                _index = index;
            }

            public new uint GetUserStringIndex(string value) => _index;
        }
    }
}
