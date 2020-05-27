using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using AsmResolver.DotNet.Extensions.Memory;
using Xunit;

namespace AsmResolver.DotNet.Extensions.Tests.Memory
{
    [SuppressMessage("ReSharper", "ArrangeTypeMemberModifiers")]
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
    public sealed class ExplicitLayoutTest : IClassFixture<TemporaryModuleFixture>
    {
        private readonly TemporaryModuleFixture _fixture;

        public ExplicitLayoutTest(TemporaryModuleFixture fixture)
        {
            _fixture = fixture;
        }

        [StructLayout(LayoutKind.Explicit)]
        struct Struct1
        {
            [FieldOffset(0)]
            int Dummy1;

            [FieldOffset(0)]
            long Dummy2;
        }

        [Theory]
        [InlineData(typeof(Struct1), 8u)]
        public void NoNest(Type type, uint expected)
        {
            var target = _fixture.LookupType(type);

            Assert.Equal(expected, target.GetImpliedMemoryLayout(IntPtr.Size == 4).Size);
        }
    }
}