using System;
using System.Linq;
using AsmResolver.DotNet.Memory;
using Xunit;

namespace AsmResolver.DotNet.Tests.Memory
{
    public class FindFieldsByOffsetTest : StructLayoutTestBase
    {
        public FindFieldsByOffsetTest(CurrentModuleFixture fixture)
            : base(fixture)
        {
        }

        [Theory]
        [InlineData(typeof(SequentialTestStructs.MultipleFieldsSequentialStructDefaultPack), 0u,
            nameof(SequentialTestStructs.MultipleFieldsSequentialStructDefaultPack.IntField))]
        [InlineData(typeof(SequentialTestStructs.MultipleFieldsSequentialStructDefaultPack), 8u,
            nameof(SequentialTestStructs.MultipleFieldsSequentialStructDefaultPack.LongField))]
        public void FindExistingFieldByOffset(Type type, uint offset, string expectedField)
        {
            var definition = (TypeDefinition) Module.LookupMember(type.MetadataToken);
            var layout = definition.GetImpliedMemoryLayout(false);

            Assert.True(layout.TryGetFieldAtOffset(offset, out var foundField));
            Assert.Equal(expectedField, foundField.Field.Name);
        }

        [Theory]
        [InlineData(typeof(SequentialTestStructs.MultipleFieldsSequentialStructDefaultPack), 2u)]
        [InlineData(typeof(SequentialTestStructs.MultipleFieldsSequentialStructDefaultPack), 4u)]
        public void FindNonExistingFieldByOffset(Type type, uint offset)
        {
            var definition = (TypeDefinition) Module.LookupMember(type.MetadataToken);
            var layout = definition.GetImpliedMemoryLayout(false);

            Assert.False(layout.TryGetFieldAtOffset(offset, out _));
        }

        [Fact]
        public void FindFieldPathOnFlatStructShouldReturnSingleElementList()
        {
            var definition = (TypeDefinition) Module.LookupMember(
                typeof(SequentialTestStructs.MultipleFieldsSequentialStructDefaultPack).MetadataToken);
            var layout = definition.GetImpliedMemoryLayout(false);

            Assert.True(layout.TryGetFieldPath(8, out var path));
            Assert.Equal(
                new[] {nameof(SequentialTestStructs.MultipleFieldsSequentialStructDefaultPack.LongField)},
                path.Select(f => f.Field.Name));
        }

        [Fact]
        public void FindExactFieldPathOnNestedStructShouldReturnCorrectPath()
        {
            var definition = (TypeDefinition) Module.LookupMember(
                typeof(MixedTestStructs.Struct3).MetadataToken);
            var layout = definition.GetImpliedMemoryLayout(false);

            Assert.True(layout.TryGetFieldPath(20, out var path));
            Assert.Equal(
                new[]
                {
                    nameof(MixedTestStructs.Struct3.Nest2),
                    nameof(MixedTestStructs.Struct2.Nest1),
                    nameof(MixedTestStructs.Struct1.Dummy1),
                },
                path.Select(f => f.Field.Name));
        }

        [Fact]
        public void FindNonExactFieldPathOnNestedStructShouldReturnCorrectPath()
        {
            var definition = (TypeDefinition) Module.LookupMember(
                typeof(MixedTestStructs.Struct3).MetadataToken);
            var layout = definition.GetImpliedMemoryLayout(false);

            Assert.False(layout.TryGetFieldPath(21, out var path));
            Assert.Equal(
                new[]
                {
                    nameof(MixedTestStructs.Struct3.Nest2),
                    nameof(MixedTestStructs.Struct2.Nest1),
                    nameof(MixedTestStructs.Struct1.Dummy1),
                },
                path.Select(f => f.Field.Name));
        }
    }
}