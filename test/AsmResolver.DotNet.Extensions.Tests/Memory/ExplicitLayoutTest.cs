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

        [StructLayout(LayoutKind.Explicit)]
        struct Struct2
        {
            [FieldOffset(0)]
            int Dummy1;

            [FieldOffset(12)]
            int Dummy2;
        }

        [StructLayout(LayoutKind.Explicit)]
        unsafe struct Struct3
        {
            [FieldOffset(0)]
            double Dummy1;

            [FieldOffset(16)]
            fixed byte Dummy2[4];
        }

        [StructLayout(LayoutKind.Explicit)]
        struct Struct4
        {
            [FieldOffset(0)]
            ulong Dummy1;

            [FieldOffset(8)]
            ConsoleKey Dummy2;
        }

        [Theory]
        [InlineData(typeof(Struct1), 8u, new uint[] { 0, 0 })]
        [InlineData(typeof(Struct2), 16u, new uint[] { 0, 12 })]
        [InlineData(typeof(Struct3), 24u, new uint[] { 0, 16 })]
        [InlineData(typeof(Struct4), 16u, new uint[] { 0, 8 })]
        public void NoNest(Type type, uint expectedSize, uint[] fieldOffsets)
        {
            var target = _fixture.LookupType(type);
            var layout = target.GetImpliedMemoryLayout();
            
            Assert.Equal(expectedSize, layout.Size);
            for (var i = 0; i < target.Fields.Count; i++)
            {
                Assert.Equal(fieldOffsets[i], layout.GetFieldOffset(target.Fields[i]));
            }
        }
    }
}