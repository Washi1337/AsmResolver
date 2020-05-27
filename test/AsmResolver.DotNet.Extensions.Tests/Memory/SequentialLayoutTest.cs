using System;
using System.Diagnostics.CodeAnalysis;
using AsmResolver.DotNet.Extensions.Memory;
using Xunit;
#pragma warning disable 169

namespace AsmResolver.DotNet.Extensions.Tests.Memory
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "ArrangeTypeMemberModifiers")]
    public sealed class SequentialLayoutTest : IClassFixture<TemporaryModuleFixture>
    {
        private readonly TemporaryModuleFixture _fixture;

        public SequentialLayoutTest(TemporaryModuleFixture fixture)
        {
            _fixture = fixture;
        }

        struct Struct1
        {
            int Dummy1;

            int Dummy2;
        }

        struct Struct2
        {
            long Dummy1;

            long Dummy2;
        }

        unsafe struct Struct3
        {
            long Dummy1;
            
            fixed byte Dummy3[4];
        }
        
        [Theory]
        [InlineData(typeof(Struct1), 8)]
        [InlineData(typeof(Struct2), 16)]
        [InlineData(typeof(Struct3), 16)]
        public void NoNest(Type type, uint size)
        {
            var target = _fixture.LookupType(type);
            
            Assert.Equal(size, target.GetImpliedMemoryLayout(IntPtr.Size == 4).Size);
        }
    }
}