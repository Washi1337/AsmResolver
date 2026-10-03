using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AsmResolver.Collections;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Parsing;
using AsmResolver.PE.DotNet.Metadata.Tables;
using Xunit;

namespace AsmResolver.DotNet.Tests
{
    public class TopLevelTypeResolutionTest
    {
        private static ModuleDefinition CreateModule(string assemblyName = "Local", RuntimeContext? context = null)
        {
            context ??= new RuntimeContext(DotNetRuntimeInfo.NetCoreApp(10, 0));
            var assembly = new AssemblyDefinition(assemblyName, new Version(1, 0, 0, 0));
            var module = new ModuleDefinition(assemblyName + ".dll", context.TargetRuntime);
            assembly.Modules.Add(module);
            context.AddAssembly(assembly);

            for (int i = 0; i < 64; i++)
                AddType(module, "Unused" + i);

            return module;
        }

        private static TypeDefinition AddType(ModuleDefinition module, string name, string? ns = "N")
        {
            var type = new TypeDefinition(ns, name, TypeAttributes.Public, module.CorLibTypeFactory.Object.Type);
            module.TopLevelTypes.Add(type);
            return type;
        }

        private static (ResolutionStatus Status, TypeDefinition? Definition) Resolve(
            ModuleDefinition module, string? ns, string name)
        {
            var reference = new TypeReference(module, module, ns, name);
            var status = reference.Resolve(module.RuntimeContext, out var definition);
            return (status, definition);
        }

        [Fact]
        public void IndexTracksCollectionMutationsAndFirstMatch()
        {
            var module = CreateModule();
            var first = AddType(module, "Duplicate");
            var second = AddType(module, "Duplicate");
            Assert.Same(first, Resolve(module, "N", "Duplicate").Definition);

            var earlier = new TypeDefinition("N", "Duplicate", TypeAttributes.Public);
            module.TopLevelTypes.Insert(1, earlier);
            Assert.Same(earlier, Resolve(module, "N", "Duplicate").Definition);

            module.TopLevelTypes.RemoveAt(1);
            Assert.Same(first, Resolve(module, "N", "Duplicate").Definition);

            var replacement = new TypeDefinition("N", "Duplicate", TypeAttributes.Public);
            module.TopLevelTypes[module.TopLevelTypes.IndexOf(first)] = replacement;
            Assert.Same(replacement, Resolve(module, "N", "Duplicate").Definition);

            replacement.Name = "Renamed";
            Assert.Same(second, Resolve(module, "N", "Duplicate").Definition);
            Assert.Same(replacement, Resolve(module, "N", "Renamed").Definition);

            var preceding = module.TopLevelTypes[1];
            preceding.Name = "Duplicate";
            Assert.Same(preceding, Resolve(module, "N", "Duplicate").Definition);
            preceding.Name = "Unused0";

            var prefix = new TypeDefinition("N", "Duplicate", TypeAttributes.Public);
            Assert.IsAssignableFrom<LazyList<TypeDefinition>>(module.TopLevelTypes).AddRange([prefix]);
            Assert.Same(second, Resolve(module, "N", "Duplicate").Definition);

            module.TopLevelTypes.Remove(second);
            Assert.Same(prefix, Resolve(module, "N", "Duplicate").Definition);

            module.TopLevelTypes.Clear();
            Assert.Equal(ResolutionStatus.TypeNotFound, Resolve(module, "N", "Duplicate").Status);
            module.TopLevelTypes.Add(prefix);
            Assert.Same(prefix, Resolve(module, "N", "Duplicate").Definition);
        }

        [Fact]
        public void IndexTracksNamesNamespacesMissesAndTypeShape()
        {
            var module = CreateModule();
            Assert.Equal(ResolutionStatus.TypeNotFound, Resolve(module, "N", "New").Status);

            var type = AddType(module, "New");
            Assert.Same(type, Resolve(module, "N", "New").Definition);
            type.Name = "Different";
            Assert.Equal(ResolutionStatus.TypeNotFound, Resolve(module, "N", "New").Status);
            Assert.Same(type, Resolve(module, "N", "Different").Definition);

            type.Namespace = "Other";
            Assert.Equal(ResolutionStatus.TypeNotFound, Resolve(module, "N", "Different").Status);
            Assert.Same(type, Resolve(module, "Other", "Different").Definition);

            type.Namespace = "";
            Assert.Same(type, Resolve(module, null, "Different").Definition);
            Assert.Equal(ResolutionStatus.TypeNotFound, Resolve(module, "other", "Different").Status);

            Assert.False(TypeNameParser.Parse(module, "Different").IsValueType);
            type.BaseType = module.CorLibTypeFactory.CorLibScope.CreateTypeReference("System", "ValueType");
            Assert.True(TypeNameParser.Parse(module, "Different").IsValueType);
        }

        [Fact]
        public void NestedRenameKeepsTopLevelIndexAndNestedScanCurrent()
        {
            var module = CreateModule();
            var outer = AddType(module, "Outer");
            var nested = new TypeDefinition(null, "Nested", TypeAttributes.NestedPublic);
            outer.NestedTypes.Add(nested);

            Assert.Same(outer, Resolve(module, "N", "Outer").Definition);
            Assert.Same(nested, module.CreateTypeReference("N", "Outer")
                .CreateTypeReference("Nested").Resolve(module.RuntimeContext));

            nested.Name = "Renamed";

            Assert.Same(outer, Resolve(module, "N", "Outer").Definition);
            Assert.Equal(ResolutionStatus.TypeNotFound, module.CreateTypeReference("N", "Outer")
                .CreateTypeReference("Nested").Resolve(module.RuntimeContext, out _));
            Assert.Same(nested, module.CreateTypeReference("N", "Outer")
                .CreateTypeReference("Renamed").Resolve(module.RuntimeContext));
        }

        [Fact]
        public void MissingLocalTypeStillUsesOrderedExportedTypes()
        {
            var module = CreateModule();
            var targetModule = CreateModule("Target", module.RuntimeContext);
            var target = AddType(targetModule, "Forwarded");

            var forwarder = new ExportedType(
                new AssemblyReference(targetModule.Assembly!), "N", "Forwarded");
            module.ExportedTypes.Add(forwarder);
            Assert.Same(target, Resolve(module, "N", "Forwarded").Definition);

            var local = AddType(module, "Forwarded");
            Assert.Same(local, Resolve(module, "N", "Forwarded").Definition);
            module.TopLevelTypes.Remove(local);
            Assert.Same(target, Resolve(module, "N", "Forwarded").Definition);

            forwarder.Implementation = new AssemblyReference(module.Assembly!);
            Assert.Equal(ResolutionStatus.TypeNotFound, Resolve(module, "N", "Forwarded").Status);
            forwarder.Implementation = new AssemblyReference(targetModule.Assembly!);
            Assert.Same(target, Resolve(module, "N", "Forwarded").Definition);

            var missingFirst = new ExportedType(new AssemblyReference(module.Assembly!), "N", "Forwarded");
            module.ExportedTypes.Insert(0, missingFirst);
            Assert.Equal(ResolutionStatus.TypeNotFound, Resolve(module, "N", "Forwarded").Status);
            module.ExportedTypes.Remove(missingFirst);
            Assert.Same(target, Resolve(module, "N", "Forwarded").Definition);
        }

        [Fact]
        public void ParallelColdLookupsStayModuleLocal()
        {
            var firstModule = CreateModule("First");
            var secondModule = CreateModule("Second");
            var first = AddType(firstModule, "Target");
            var second = AddType(secondModule, "Target");

            Parallel.For(0, 512, i =>
            {
                var module = i % 2 == 0 ? firstModule : secondModule;
                var expected = i % 2 == 0 ? first : second;
                if (Resolve(module, "N", "Target") is not { Status: ResolutionStatus.Success, Definition: var actual }
                    || !ReferenceEquals(expected, actual))
                {
                    throw new InvalidOperationException("Parallel type resolution returned the wrong definition.");
                }
            });
        }

        [Fact]
        public void ParallelSerializedColdLookupsUseOneModuleIndex()
        {
            var module = CreateModule();
            using var stream = new MemoryStream();
            module.Write(stream);
            var loaded = ModuleDefinition.FromBytes(stream.ToArray());

            Parallel.For(0, 256, i =>
            {
                string name = "Unused" + (i % 64);
                var result = Resolve(loaded, "N", name);
                if (result is not { Status: ResolutionStatus.Success, Definition: { } definition }
                    || definition.Name != name || !ReferenceEquals(definition.DeclaringModule, loaded))
                {
                    throw new InvalidOperationException("Parallel serialized lookup returned the wrong definition.");
                }
            });
        }

        [Fact]
        public void IndexUsesUtf8NameAndNamespaceEquality()
        {
            var module = CreateModule();
            var invalidName = new Utf8String([0xFF]);
            var differentInvalidName = new Utf8String([0xFE]);
            var invalidNamespace = new Utf8String([0xFF]);
            var type = new TypeDefinition(invalidNamespace, invalidName, TypeAttributes.Public);
            module.TopLevelTypes.Add(type);

            var reference = new TypeReference(module, module,
                new Utf8String([0xFF]), new Utf8String([0xFF]));
            Assert.Same(type, reference.Resolve(module.RuntimeContext));
            Assert.Equal(ResolutionStatus.TypeNotFound, new TypeReference(
                module, module, invalidNamespace, differentInvalidName).Resolve(module.RuntimeContext, out _));
            Assert.Equal(ResolutionStatus.TypeNotFound, new TypeReference(
                module, module, new Utf8String([0xFE]), invalidName).Resolve(module.RuntimeContext, out _));
        }

        [Fact]
        public void OverriddenCollectionKeepsOrderedScan()
        {
            var module = new ListBackedModule();
            var assembly = new AssemblyDefinition("ListBacked", new Version(1, 0, 0, 0));
            assembly.Modules.Add(module);
            var context = new RuntimeContext(DotNetRuntimeInfo.NetCoreApp(10, 0));
            context.AddAssembly(assembly);
            var first = new TypeDefinition("N", "Target", TypeAttributes.Public);
            module.TopLevelTypes.Add(first);
            Assert.Same(first, Resolve(module, "N", "Target").Definition);

            var earlier = new TypeDefinition("N", "Target", TypeAttributes.Public);
            module.TopLevelTypes.Insert(0, earlier);
            Assert.Same(earlier, Resolve(module, "N", "Target").Definition);
        }

        [Fact]
        public void SerializedTypeArgumentsUseIndexAndRemainMutable()
        {
            var module = CreateModule();
            var lookup = AddType(module, "Lookup");
            var typeSignature = module.CorLibTypeFactory.CorLibScope
                .CreateTypeReference("System", "Type").ToTypeSignature(false);
            var constructor = module.CorLibTypeFactory.CorLibScope
                .CreateTypeReference("N", "PairAttribute")
                .CreateMemberReference(".ctor", MethodSignature.CreateInstance(
                    module.CorLibTypeFactory.Void, [typeSignature, typeSignature]));

            for (int i = 0; i < 100; i++)
            {
                var first = AddType(module, "Class" + i);
                var second = AddType(module, "Interface" + i);
                lookup.CustomAttributes.Add(new CustomAttribute(
                    constructor,
                    new CustomAttributeSignature(
                        new CustomAttributeArgument(typeSignature, first.ToTypeSignature(false)),
                        new CustomAttributeArgument(typeSignature, second.ToTypeSignature(false)))));
            }

            using var stream = new MemoryStream();
            module.Write(stream);
            var loaded = ModuleDefinition.FromBytes(stream.ToArray());
            var attributes = loaded.TopLevelTypes[65].CustomAttributes;

            for (int i = 0; i < attributes.Count; i++)
            {
                var args = attributes[i].Signature!.FixedArguments;
                Assert.Equal(2, args.Count);
                Assert.Equal("Class" + i, args[0].Element is TypeSignature first ? first.Name : null);
                Assert.Equal("Interface" + i, args[1].Element is TypeSignature second ? second.Name : null);
            }

            var renamed = loaded.TopLevelTypes[66];
            renamed.Name = "NewName";
            Assert.Same(renamed, Resolve(loaded, "N", "NewName").Definition);
            Assert.Equal(ResolutionStatus.TypeNotFound, Resolve(loaded, "N", "Class0").Status);

            var signature = new CustomAttributeSignature(new CustomAttributeArgument(typeSignature, (object?) null));
            attributes[0].Signature = signature;
            Assert.Same(signature, attributes[0].Signature);
            signature.FixedArguments[0].Elements[0] = TypeNameParser.Parse(loaded, "NewName");
            Assert.Equal("NewName", Assert.IsAssignableFrom<TypeSignature>(
                signature.FixedArguments[0].Element).Name);
        }

        private sealed class ListBackedModule : ModuleDefinition
        {
            public ListBackedModule()
                : base("ListBacked.dll")
            {
            }

            protected override IList<TypeDefinition> GetTopLevelTypes() => new List<TypeDefinition>();
        }
    }
}
