using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using AsmResolver.DotNet.Extensions.Memory;
using Xunit;

namespace AsmResolver.DotNet.Extensions.Tests.Memory
{
    [SuppressMessage("ReSharper", "UnusedType.Local")]
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
    [SuppressMessage("ReSharper", "ArrangeTypeMemberModifiers")]
    public sealed class AutoLayoutTest : IClassFixture<TemporaryModuleFixture>
    {
        private readonly TemporaryModuleFixture _fixture;
        private readonly ModuleDefinition _bcl;
        
        public AutoLayoutTest(TemporaryModuleFixture fixture)
        {
            _fixture = fixture;
            _bcl = ModuleDefinition.FromFile(typeof(DateTimeOffset).Assembly.Location);
        }

        [StructLayout(LayoutKind.Auto)]
        struct ComplexAutoLayoutStruct
        {
            int Dummy1;

            long Dummy2;
        }
            

        [Fact]
        public void ComplexAutoLayout()
        {
            var target = _fixture.LookupType(typeof(ComplexAutoLayoutStruct));

            Assert.Throws<TypeMemoryLayoutDetectionException>(() => target.GetImpliedMemoryLayout());
        }

        [Theory]
        [InlineData("AttributeTargets", 4u)]
        [InlineData("DateTimeKind", 4u)]
        [InlineData("DayOfWeek", 4u)]
        [InlineData("TypeCode", 4u)]
        public void Enums(string name, uint expected)
        {
            var mscorlib = ((AssemblyReference) _fixture.Module.CorLibTypeFactory.CorLibScope).Resolve().ManifestModule;
            var target = mscorlib.ExportedTypes.Single(t => t.Name == name).Resolve();
            
            Assert.Equal(expected, target.GetImpliedMemoryLayout().Size);
        }
    }
}