using System.Diagnostics.CodeAnalysis;
using AsmResolver.DotNet.Extensions.Memory;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Xunit;

namespace AsmResolver.DotNet.Extensions.Tests.Memory
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "ArrangeTypeMemberModifiers")]
    public sealed class CyclicStructTest : IClassFixture<TemporaryModuleFixture>
    {
        private readonly TemporaryModuleFixture _fixture;

        public CyclicStructTest(TemporaryModuleFixture fixture)
        {
            _fixture = fixture;
        }
        
        struct Normal
        {
            // We will place a field of type Abnormal here
        }

        struct Abnormal
        {
            #pragma warning disable 169
            Normal Dummy;
            #pragma warning restore 169
        }
        
        [Fact]
        public void CyclicStructShouldThrow()
        {
            var normal = _fixture.LookupType(typeof(Normal));
            var abnormal = _fixture.LookupType(typeof(Abnormal));

            var signature = FieldSignature.CreateInstance(abnormal.ToTypeSignature());
            normal.Fields.Add(new FieldDefinition("Abnormal", FieldAttributes.Private, signature));

            Assert.Throws<TypeMemoryLayoutDetectionException>(() => normal.GetImpliedMemoryLayout(false));
        }
    }
}