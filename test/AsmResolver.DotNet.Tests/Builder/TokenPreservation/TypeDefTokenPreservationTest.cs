using System;
using System.Linq;
using AsmResolver.DotNet.Builder;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.TestCases.Fields;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables;
using Xunit;

namespace AsmResolver.DotNet.Tests.Builder.TokenPreservation
{
    public class TypeDefTokenPreservationTest : TokenPreservationTestBase
    {
        [Fact]
        public void PreserveTypeDefsNoChangeShouldResultInSameTypeDefs()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_NetCore, TestReaderParameters);
            var originalTypeDefs = GetMembers<TypeDefinition>(module, TableIndex.TypeDef);

            var newModule = RebuildAndReloadModule(module, MetadataBuilderFlags.PreserveTypeDefinitionIndices);
            var newTypeDefs = GetMembers<TypeDefinition>(newModule, TableIndex.TypeDef);

            Assert.Equal(originalTypeDefs, newTypeDefs.Take(originalTypeDefs.Count), Comparer);
        }

        [Fact]
        public void PreserveTypeDefsAddTypeToEndShouldResultInSameTypeDefs()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_NetCore, TestReaderParameters);
            var originalTypeDefs = GetMembers<TypeDefinition>(module, TableIndex.TypeDef);

            var newType = new TypeDefinition("Namespace", "Name", TypeAttributes.Public);
            module.TopLevelTypes.Add(newType);

            var newModule = RebuildAndReloadModule(module, MetadataBuilderFlags.PreserveTypeDefinitionIndices);
            var newTypeDefs = GetMembers<TypeDefinition>(newModule, TableIndex.TypeDef);

            Assert.Equal(originalTypeDefs, newTypeDefs.Take(originalTypeDefs.Count), Comparer);
            Assert.Contains(newType, newTypeDefs, Comparer);
        }

        [Fact]
        public void PreserveTypeDefsInsertTypeShouldResultInSameTypeDefs()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_NetCore, TestReaderParameters);
            var originalTypeDefs = GetMembers<TypeDefinition>(module, TableIndex.TypeDef);

            var newType = new TypeDefinition("Namespace", "Name", TypeAttributes.Public);
            module.TopLevelTypes.Insert(1, newType);

            var newModule = RebuildAndReloadModule(module, MetadataBuilderFlags.PreserveTypeDefinitionIndices);
            var newTypeDefs = GetMembers<TypeDefinition>(newModule, TableIndex.TypeDef);

            Assert.Equal(originalTypeDefs, newTypeDefs.Take(originalTypeDefs.Count), Comparer);
            Assert.Contains(newType, newTypeDefs, Comparer);
        }

        [Fact]
        public void PreserveTypeDefsRemoveTypeShouldStuffTypeDefSlots()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_NetCore, TestReaderParameters);
            for (int i = 0; i < 10; i++)
                module.TopLevelTypes.Add(new TypeDefinition("Namespace", $"Type{i.ToString()}", TypeAttributes.Public));
            module = RebuildAndReloadModule(module, MetadataBuilderFlags.None);

            const int indexToRemove = 2;

            var originalTypeDefs = GetMembers<TypeDefinition>(module, TableIndex.TypeDef);
            module.TopLevelTypes.RemoveAt(indexToRemove);

            var newModule = RebuildAndReloadModule(module, MetadataBuilderFlags.PreserveTypeDefinitionIndices);
            var newTypeDefs = GetMembers<TypeDefinition>(newModule, TableIndex.TypeDef);

            Assert.All(Enumerable.Range(0, originalTypeDefs.Count), i =>
            {
                if (i != indexToRemove)
                    Assert.Equal(originalTypeDefs[i], newTypeDefs[i], Comparer);
            });
        }

        [Fact]
        public void PreserveTypeDefsAfterInsertingNewModuleTypeShouldThrow()
        {
            // Prepare.
            var module = new ModuleDefinition("SomeModule", KnownCorLibs.SystemRuntime_v8_0_0_0);
            module = RebuildAndReloadModule(module, MetadataBuilderFlags.None);

            // Insert new module type, but keep original module type around.
            module.GetOrCreateModuleType().Name = "<NotModule>";
            var newModuleType = module.GetOrCreateModuleType();

            // Rebuild with type preservation should conflict.
            var exception = Assert.Throws<AggregateException>(() => RebuildAndReloadModule(module, MetadataBuilderFlags.PreserveTypeDefinitionIndices));
            Assert.Contains(exception.InnerExceptions, ex =>
            {
                return ex is MetadataTokenConflictException conflict
                    && (conflict.Member1 == newModuleType || conflict.Member2 == newModuleType)
                    && conflict.Token.Rid == 1;
            });
        }

        [Fact]
        public void PreserveTypeDefsAfterInsertingNewModuleTypeAndRemovingOldShouldNotThrow()
        {
            // Prepare.
            var module = new ModuleDefinition("SomeModule", KnownCorLibs.SystemRuntime_v8_0_0_0);
            var moduleType = module.GetOrCreateModuleType();
            moduleType.Fields.Add(new FieldDefinition("SomeField", FieldAttributes.Static, module.CorLibTypeFactory.Int32));
            module = RebuildAndReloadModule(module, MetadataBuilderFlags.None);

            // Insert new module type, and remove original module type.
            module.TopLevelTypes.Remove(module.GetModuleType());
            moduleType = module.GetOrCreateModuleType();
            moduleType.Fields.Add(new FieldDefinition("OtherField", FieldAttributes.Static, module.CorLibTypeFactory.Int16));

            // Rebuild should not conflict.
            var newModule = RebuildAndReloadModule(module, MetadataBuilderFlags.PreserveTypeDefinitionIndices);

            // Assert that the module type was replaced with the new one.
            var newModuleType = newModule.GetModuleType();
            Assert.NotNull(newModuleType);
            var fields = Assert.Single(newModuleType.Fields);
            Assert.Equal("OtherField", fields.Name);
        }

        [Fact]
        public void PreserveInvalidNestedTypeOrderingShouldNonFatallyThrow()
        {
            // Prepare.
            var module = new ModuleDefinition("SomeModule", KnownCorLibs.SystemRuntime_v8_0_0_0);
            module.TopLevelTypes.Add(new TypeDefinition(null, "Type1", TypeAttributes.Public));
            module.TopLevelTypes.Add(new TypeDefinition(null, "Type2", TypeAttributes.Public));
            module = RebuildAndReloadModule(module, MetadataBuilderFlags.None);

            // Move type1 into type2.
            var type1 = module.TopLevelTypes[1];
            var type2 = module.TopLevelTypes[2];
            module.TopLevelTypes.Remove(type1);
            type2.NestedTypes.Add(type1);
            type1.IsNestedPublic = true;

            // Rebuild with type token preservation should not fatally throw.
            var bag = new DiagnosticBag();
            var newModule = RebuildAndReloadModule(module, MetadataBuilderFlags.PreserveTypeDefinitionIndices, bag);

            // Assert exception was recorded.
            Assert.Contains(bag.Exceptions, ex => ex is MetadataBuilderException);

            // Assert that nested type relation is preserved.
            var newType2 = newModule.TopLevelTypes.First(t => t.Name == type2.Name);
            Assert.Contains(newType2.NestedTypes, t => t.Name == type1.Name);
        }

        [Fact]
        public void DontPreserveBaseTypeAsTypeDefTokenIfExternal()
        {
            // Prepare external module.
            var externalAssembly = new AssemblyDefinition("SomeAssembly", new Version(1, 0, 0, 0));
            var externalModule = new ModuleDefinition("SomeModule", KnownCorLibs.SystemRuntime_v8_0_0_0);
            externalModule.TopLevelTypes.Add(new TypeDefinition(null, "Type1", TypeAttributes.Public));
            externalAssembly.Modules.Add(externalModule);
            externalModule = RebuildAndReloadModule(externalModule, MetadataBuilderFlags.None);
            var externalType = externalModule.TopLevelTypes[1];

            // Prepare new module.
            var module = new ModuleDefinition("SomeModule", KnownCorLibs.SystemRuntime_v8_0_0_0);
            module.TopLevelTypes.Add(new TypeDefinition(null, "Type1", TypeAttributes.Public));
            module = RebuildAndReloadModule(module, MetadataBuilderFlags.None);

            // Set external typedef as base type
            module.TopLevelTypes.Add(new TypeDefinition(null, "Type2", TypeAttributes.Class, externalType));

            // Rebuild
            var bag = new DiagnosticBag();
            var newModule = RebuildAndReloadModule(module, MetadataBuilderFlags.PreserveTypeDefinitionIndices, bag);

            // Validate we turned the typedef into a typeref.
            var newType = Assert.IsAssignableFrom<TypeReference>(newModule.TopLevelTypes[2].BaseType);
            Assert.Equal<ITypeDefOrRef>(newType, externalType, Comparer);
        }

        [Fact]
        public void DontPreserveOperandAsTypeDefTokenIfExternal()
        {
            // Prepare external module.
            var externalAssembly = new AssemblyDefinition("SomeAssembly", new Version(1, 0, 0, 0));
            var externalModule = new ModuleDefinition("SomeModule", KnownCorLibs.SystemRuntime_v8_0_0_0);
            externalModule.TopLevelTypes.Add(new TypeDefinition(null, "Type1", TypeAttributes.Public));
            externalAssembly.Modules.Add(externalModule);
            externalModule = RebuildAndReloadModule(externalModule, MetadataBuilderFlags.None);
            var externalType = externalModule.TopLevelTypes[1];

            // Prepare new module.
            var module = new ModuleDefinition("SomeModule", KnownCorLibs.SystemRuntime_v8_0_0_0);
            module.TopLevelTypes.Add(new TypeDefinition(null, "Type1", TypeAttributes.Public));
            module = RebuildAndReloadModule(module, MetadataBuilderFlags.None);

            // Set external typedef as an operand.
            module.GetOrCreateModuleType().Methods.Add(
                new MethodDefinition("Foo", MethodAttributes.Static, MethodSignature.CreateStatic(module.CorLibTypeFactory.Void))
                {
                    CilMethodBody = new CilMethodBody
                    {
                        Instructions =
                        {
                            { CilOpCodes.Ldtoken, externalType },
                            CilOpCodes.Pop,
                            CilOpCodes.Ret
                        }
                    }
                }
            );

            // Rebuild
            var bag = new DiagnosticBag();
            var newModule = RebuildAndReloadModule(module, MetadataBuilderFlags.PreserveTypeDefinitionIndices, bag);
            var newMethod = newModule.GetModuleType()!.Methods.First(m => m.Name == "Foo");

            // Validate we turned the typedef into a typeref.
            var newType = Assert.IsAssignableFrom<TypeReference>(newMethod.CilMethodBody!.Instructions[0].Operand);
            Assert.Equal<ITypeDefOrRef>(newType, externalType, Comparer);
        }
    }
}
