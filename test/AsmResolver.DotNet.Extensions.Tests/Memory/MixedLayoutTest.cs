using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AsmResolver.DotNet.Extensions.Memory;
using AsmResolver.DotNet.Signatures.Types;
using Xunit;
#pragma warning disable 169

namespace AsmResolver.DotNet.Extensions.Tests.Memory
{
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
    [SuppressMessage("ReSharper", "ArrangeTypeMemberModifiers")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public sealed class MixedLayoutTest : IClassFixture<TemporaryModuleFixture>
    {
        private readonly TemporaryModuleFixture _fixture;
        
        public MixedLayoutTest(TemporaryModuleFixture fixture)
        {
            _fixture = fixture;
        }

        [StructLayout(LayoutKind.Sequential, Size = 17)]
        struct Struct1
        {
            int Dummy1;
        }

        [StructLayout(LayoutKind.Sequential, Size = 23, Pack = 2)]
        struct Struct2
        {
            Struct1 Nest1;
        }

        [StructLayout(LayoutKind.Sequential, Size = 87, Pack = 64)]
        struct Struct3
        {
            Struct1 Nest1;

            Struct2 Nest2;
        }

        [StructLayout(LayoutKind.Sequential)]
        unsafe struct Struct4
        {
            Struct3 Nest1;
            
            byte Dummy2;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        struct Struct5
        {
            Struct4 Nest1;

            double Dummy1;
        }

        [Fact]
        public void ExtremeNesting()
        {
            var target = _fixture.LookupType(typeof(Struct4));
            var layout = target.GetImpliedMemoryLayout();
            
            Assert.Equal((uint) Unsafe.SizeOf<Struct4>(), layout.Size);
        }
    }
}