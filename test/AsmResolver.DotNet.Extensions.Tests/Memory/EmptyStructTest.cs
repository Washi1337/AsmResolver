using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using AsmResolver.DotNet.Extensions.Memory;
using Xunit;

namespace AsmResolver.DotNet.Extensions.Tests.Memory
{
    [SuppressMessage("ReSharper", "ArrangeTypeMemberModifiers")]
    public sealed class EmptyStructTest : IClassFixture<TemporaryModuleFixture>
    {
        private readonly TemporaryModuleFixture _fixture;
        
        public EmptyStructTest(TemporaryModuleFixture fixture)
        {
            _fixture = fixture;
        }

        struct EmptyStruct { }

        [Fact]
        public void EmptyStructShouldHaveSizeOfOne()
        {
            var target = _fixture.LookupType(typeof(EmptyStruct));
            
            Assert.Equal((uint) Unsafe.SizeOf<EmptyStruct>(), target.GetImpliedMemoryLayout().Size);
        }
    }
}