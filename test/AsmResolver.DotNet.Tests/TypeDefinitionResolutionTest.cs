using System;
using System.Collections.Generic;
using System.IO;
using AsmResolver.Collections;
using AsmResolver.PE.DotNet.Metadata.Tables;
using Xunit;

namespace AsmResolver.DotNet.Tests
{
    public class TypeDefinitionResolutionTest
    {
        // Keep lookups on the indexed path, including after a removal.
        private const int PaddingTypeCount = 64;

        private static ModuleDefinition CreateModule()
        {
            var context = new RuntimeContext(DotNetRuntimeInfo.NetCoreApp(10, 0));
            var assembly = new AssemblyDefinition("Local", new Version(1, 0, 0, 0));
            var module = new ModuleDefinition("Local.dll", context.TargetRuntime);
            assembly.Modules.Add(module);
            context.AddAssembly(assembly);
            AddPaddingTypes(module.TopLevelTypes);
            return module;
        }

        private static void AddPaddingTypes(IList<TypeDefinition> types, TypeAttributes attributes = TypeAttributes.Public)
        {
            for (int i = 0; i < PaddingTypeCount; i++)
                types.Add(new TypeDefinition(null, "Unused" + i, attributes));
        }

        [Fact]
        public void InsertingEarlierDuplicateShouldChangeResolvedDefinition()
        {
            var module = CreateModule();
            var first = new TypeDefinition("N", "Duplicate", TypeAttributes.Public);
            var second = new TypeDefinition("N", "Duplicate", TypeAttributes.Public);
            module.TopLevelTypes.Add(first);
            module.TopLevelTypes.Add(second);
            Assert.Same(first, module.CreateTypeReference("N", "Duplicate").Resolve(module.RuntimeContext));

            var earlier = new TypeDefinition("N", "Duplicate", TypeAttributes.Public);
            module.TopLevelTypes.Insert(0, earlier);

            Assert.Same(earlier, module.CreateTypeReference("N", "Duplicate").Resolve(module.RuntimeContext));
        }

        [Fact]
        public void AddingRangeAfterLookupMissShouldResolveAddedType()
        {
            var module = CreateModule();
            Assert.False(module.CreateTypeReference("N", "Target").TryResolve(module.RuntimeContext, out _));

            var type = new TypeDefinition("N", "Target", TypeAttributes.Public);
            Assert.IsAssignableFrom<LazyList<TypeDefinition>>(module.TopLevelTypes).AddRange([type]);

            Assert.Same(type, module.CreateTypeReference("N", "Target").Resolve(module.RuntimeContext));
        }

        [Fact]
        public void ReplacingTypeShouldUpdateResolvedNames()
        {
            var module = CreateModule();
            var original = new TypeDefinition("N", "Original", TypeAttributes.Public);
            module.TopLevelTypes.Add(original);
            Assert.Same(original, module.CreateTypeReference("N", "Original").Resolve(module.RuntimeContext));

            var replacement = new TypeDefinition("N", "Replacement", TypeAttributes.Public);
            module.TopLevelTypes[module.TopLevelTypes.IndexOf(original)] = replacement;

            Assert.False(module.CreateTypeReference("N", "Original").TryResolve(module.RuntimeContext, out _));
            Assert.Same(replacement, module.CreateTypeReference("N", "Replacement").Resolve(module.RuntimeContext));
        }

        [Fact]
        public void RemovingTypeShouldPreventResolution()
        {
            var module = CreateModule();
            var type = new TypeDefinition("N", "Target", TypeAttributes.Public);
            module.TopLevelTypes.Add(type);
            Assert.Same(type, module.CreateTypeReference("N", "Target").Resolve(module.RuntimeContext));

            module.TopLevelTypes.Remove(type);

            Assert.False(module.CreateTypeReference("N", "Target").TryResolve(module.RuntimeContext, out _));
        }

        [Fact]
        public void ChangingNamespaceShouldUpdateResolvedNames()
        {
            var module = CreateModule();
            var type = new TypeDefinition("Original", "Target", TypeAttributes.Public);
            module.TopLevelTypes.Add(type);
            Assert.Same(type, module.CreateTypeReference("Original", "Target").Resolve(module.RuntimeContext));

            type.Namespace = "Replacement";

            Assert.False(module.CreateTypeReference("Original", "Target").TryResolve(module.RuntimeContext, out _));
            Assert.Same(type, module.CreateTypeReference("Replacement", "Target").Resolve(module.RuntimeContext));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void RenamingNestedTypeShouldUpdateResolvedNames(bool rebuild)
        {
            var module = CreateModule();
            var outer = new TypeDefinition("N", "Outer", TypeAttributes.Public);
            module.TopLevelTypes.Add(outer);
            AddPaddingTypes(outer.NestedTypes, TypeAttributes.NestedPublic);
            outer.NestedTypes.Add(new TypeDefinition(null, "Nested", TypeAttributes.NestedPublic));

            if (rebuild)
            {
                using var stream = new MemoryStream();
                module.Write(stream);
                module = ModuleDefinition.FromBytes(stream.ToArray());
            }

            var outerReference = module.CreateTypeReference("N", "Outer");
            var nested = outerReference.CreateTypeReference("Nested").Resolve(module.RuntimeContext);

            nested.Name = "Renamed";

            Assert.False(outerReference.CreateTypeReference("Nested").TryResolve(module.RuntimeContext, out _));
            Assert.Same(nested, outerReference.CreateTypeReference("Renamed").Resolve(module.RuntimeContext));
        }

        [Fact]
        public void LookupShouldDistinguishDifferentMalformedUtf8Names()
        {
            var module = CreateModule();
            Utf8String ns = "N";
            var type = new TypeDefinition(ns, new Utf8String([0xFF]), TypeAttributes.Public);
            module.TopLevelTypes.Add(type);

            Assert.Same(type, module.CreateTypeReference(ns, new Utf8String([0xFF]))
                .Resolve(module.RuntimeContext));
            Assert.False(module.CreateTypeReference(ns, new Utf8String([0xFE]))
                .TryResolve(module.RuntimeContext, out _));
        }

        [Fact]
        public void CustomTopLevelCollectionShouldReflectInsertionsAfterLookup()
        {
            var module = new ListBackedModule();
            var assembly = new AssemblyDefinition("Custom", new Version(1, 0, 0, 0));
            assembly.Modules.Add(module);
            var context = new RuntimeContext(DotNetRuntimeInfo.NetCoreApp(10, 0));
            context.AddAssembly(assembly);
            AddPaddingTypes(module.TopLevelTypes);
            var first = new TypeDefinition("N", "Target", TypeAttributes.Public);
            module.TopLevelTypes.Add(first);
            Assert.Same(first, module.CreateTypeReference("N", "Target").Resolve(context));

            var earlier = new TypeDefinition("N", "Target", TypeAttributes.Public);
            module.TopLevelTypes.Insert(0, earlier);

            Assert.Same(earlier, module.CreateTypeReference("N", "Target").Resolve(context));
        }

        [Fact]
        public void CustomNestedCollectionShouldReflectInsertionsAfterLookup()
        {
            var module = CreateModule();
            var outer = new ListBackedType();
            module.TopLevelTypes.Add(outer);
            AddPaddingTypes(outer.NestedTypes, TypeAttributes.NestedPublic);
            var first = new TypeDefinition(null, "Target", TypeAttributes.NestedPublic);
            outer.NestedTypes.Add(first);
            var outerReference = module.CreateTypeReference("N", "Outer");
            Assert.Same(first, outerReference.CreateTypeReference("Target").Resolve(module.RuntimeContext));

            var earlier = new TypeDefinition(null, "Target", TypeAttributes.NestedPublic);
            outer.NestedTypes.Insert(0, earlier);

            Assert.Same(earlier, outerReference.CreateTypeReference("Target").Resolve(module.RuntimeContext));
        }

        private sealed class ListBackedModule : ModuleDefinition
        {
            public ListBackedModule()
                : base("Custom.dll")
            {
            }

            protected override IList<TypeDefinition> GetTopLevelTypes() => new List<TypeDefinition>();
        }

        private sealed class ListBackedType : TypeDefinition
        {
            public ListBackedType()
                : base("N", "Outer", TypeAttributes.Public)
            {
            }

            protected override IList<TypeDefinition> GetNestedTypes() => new List<TypeDefinition>();
        }
    }
}
