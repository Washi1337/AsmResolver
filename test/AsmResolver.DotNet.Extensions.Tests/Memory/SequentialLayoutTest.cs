using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
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

        [StructLayout(LayoutKind.Sequential, Pack = 32)]
        struct Struct4
        {
            long Dummy1;
        }

        [StructLayout(LayoutKind.Sequential, Size = 4)]
        struct Struct5
        {
            long Dummy1;
        }

        [StructLayout(LayoutKind.Sequential, Size = 16)]
        struct Struct6
        {
            byte Dummy1;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct Struct7
        {
            Struct6 Nest;

            int Dummy1;

            ushort Dummy2;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 64)]
        struct Struct8
        {
            Struct2 Dummy1;

            Struct7 Dummy2;

            int Dummy3;
        }
        
        [Theory]
        [InlineData(typeof(Struct1), 8u, new uint[] { 0, 4 })]
        [InlineData(typeof(Struct2), 16u, new uint[] { 0, 8 })]
        [InlineData(typeof(Struct3), 16u, new uint[] { 0, 8 })]
        [InlineData(typeof(Struct4), 8u, new uint[] { 0 })]
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

        [Theory]
        [InlineData(typeof(Struct5), 8u)]
        [InlineData(typeof(Struct6), 16u)]
        public void ExplicitSize(Type type, uint expected)
        {
            var target = _fixture.LookupType(type);
            
            Assert.Equal(expected, target.GetImpliedMemoryLayout().Size);
        }

        [Theory]
        [InlineData(typeof(Struct7), 24u)]
        [InlineData(typeof(Struct8), 48u)]
        public void Nest(Type type, uint expected)
        {
            var target = _fixture.LookupType(type);
            
            Assert.Equal(expected, target.GetImpliedMemoryLayout().Size);
        }
    }
}