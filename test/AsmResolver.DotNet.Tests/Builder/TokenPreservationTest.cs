using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AsmResolver.DotNet.Builder;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Builder;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Xunit;

namespace AsmResolver.DotNet.Tests.Builder
{
    public class TokenPreservationTest
    {
        private static readonly SignatureComparer Comparer = new SignatureComparer();
        
        private static List<TMember> GetMembers<TMember>(ModuleDefinition module, TableIndex tableIndex)
        {
            int count = module.DotNetDirectory.Metadata
                .GetStream<TablesStream>()
                .GetTable(tableIndex)
                .Count;

            var result = new List<TMember>();
            for (uint rid = 1; rid <= count; rid++)
                result.Add((TMember) module.LookupMember(new MetadataToken(tableIndex, rid)));
            return result;
        }

        private static ModuleDefinition RebuildAndReloadModule(ModuleDefinition module, MetadataBuilderFlags metadataBuilderFlags)
        {
            var builder = new ManagedPEImageBuilder
            {
                MetadataBuilderFlags = metadataBuilderFlags
            };
            
            var newImage = builder.CreateImage(module);
            return ModuleDefinition.FromImage(newImage);
        }

        [Fact]
        public void PreserveTypeRefsNoChangeShouldAtLeastHaveOriginalTypeRefs()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_NetCore);
            var originalTypeRefs = GetMembers<TypeReference>(module, TableIndex.TypeRef);
            
            var newModule = RebuildAndReloadModule(module, MetadataBuilderFlags.PreserveTypeReferenceIndices);
            var newTypeRefs = GetMembers<TypeReference>(newModule, TableIndex.TypeRef);
            
            Assert.Equal(originalTypeRefs, newTypeRefs.Take(originalTypeRefs.Count), Comparer);
        }

        [Fact]
        public void PreserveTypeRefsWithTypeRefRemovedShouldAtLeastHaveOriginalTypeRefs()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_NetCore);
            var originalTypeRefs = GetMembers<TypeReference>(module, TableIndex.TypeRef);
            
            var instructions = module.ManagedEntrypointMethod.CilMethodBody.Instructions;
            instructions.Clear();
            instructions.Add(new CilInstruction(CilOpCodes.Ret));
            
            var newModule = RebuildAndReloadModule(module, MetadataBuilderFlags.PreserveTypeReferenceIndices);
            var newTypeRefs = GetMembers<TypeReference>(newModule, TableIndex.TypeRef);
            
            Assert.Equal(originalTypeRefs, newTypeRefs.Take(originalTypeRefs.Count), Comparer);
        }

        [Fact]
        public void PreserveTypeRefsWithExtraImportShouldAtLeastHaveOriginalTypeRefs()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_NetCore);
            var originalTypeRefs = GetMembers<TypeReference>(module, TableIndex.TypeRef);
            
            var importer = new ReferenceImporter(module);
            var readKey = importer.ImportMethod(typeof(Console).GetMethod("ReadKey", Type.EmptyTypes));

            var instructions = module.ManagedEntrypointMethod.CilMethodBody.Instructions;
            instructions.RemoveAt(instructions.Count-1);
            instructions.AddRange(new[]
            {
                new CilInstruction(CilOpCodes.Call, readKey),
                new CilInstruction(CilOpCodes.Pop),
                new CilInstruction(CilOpCodes.Ret),
            });
            
            var newModule = RebuildAndReloadModule(module, MetadataBuilderFlags.PreserveTypeReferenceIndices);
            var newTypeRefs = GetMembers<TypeReference>(newModule, TableIndex.TypeRef);
            
            Assert.Equal(originalTypeRefs, newTypeRefs.Take(originalTypeRefs.Count), Comparer);
        }

        [Fact]
        public void PreserveMemberRefsNoChangeShouldAtLeastHaveOriginalMemberRefs()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_NetCore);
            var originalMemberRefs = GetMembers<MemberReference>(module, TableIndex.MemberRef);
            
            var newModule = RebuildAndReloadModule(module, MetadataBuilderFlags.PreserveMemberReferenceIndices);
            var newMemberRefs = GetMembers<MemberReference>(newModule, TableIndex.MemberRef);
            
            Assert.Equal(originalMemberRefs, newMemberRefs.Take(originalMemberRefs.Count), Comparer);
        }

        [Fact]
        public void PreserveMemberRefsWithTypeRefRemovedShouldAtLeastHaveOriginalMemberRefs()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_NetCore);
            var originalMemberRefs = GetMembers<MemberReference>(module, TableIndex.MemberRef);
            
            var instructions = module.ManagedEntrypointMethod.CilMethodBody.Instructions;
            instructions.Clear();
            instructions.Add(new CilInstruction(CilOpCodes.Ret));
            
            var newModule = RebuildAndReloadModule(module, MetadataBuilderFlags.PreserveMemberReferenceIndices);
            var newMemberRefs = GetMembers<MemberReference>(newModule, TableIndex.MemberRef);
            
            Assert.Equal(originalMemberRefs, newMemberRefs.Take(originalMemberRefs.Count), Comparer);
        }

        [Fact]
        public void PreserveMemberRefsWithExtraImportShouldAtLeastHaveOriginalMemberRefs()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_NetCore);
            var originalMemberRefs = GetMembers<MemberReference>(module, TableIndex.MemberRef);
            
            var importer = new ReferenceImporter(module);
            var readKey = importer.ImportMethod(typeof(Console).GetMethod("ReadKey", Type.EmptyTypes));

            var instructions = module.ManagedEntrypointMethod.CilMethodBody.Instructions;
            instructions.RemoveAt(instructions.Count-1);
            instructions.AddRange(new[]
            {
                new CilInstruction(CilOpCodes.Call, readKey),
                new CilInstruction(CilOpCodes.Pop),
                new CilInstruction(CilOpCodes.Ret),
            });
            
            var newModule = RebuildAndReloadModule(module, MetadataBuilderFlags.PreserveMemberReferenceIndices);
            var newMemberRefs = GetMembers<MemberReference>(newModule, TableIndex.MemberRef);
            
            Assert.Equal(originalMemberRefs, newMemberRefs.Take(originalMemberRefs.Count), Comparer);
        }

        [Fact]
        public void PreserveAssemblyRefsNoChangeShouldAtLeastHaveOriginalAssemblyRefs()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_NetCore);
            var originalAssemblyRefs = GetMembers<AssemblyReference>(module, TableIndex.AssemblyRef);
            
            var newModule = RebuildAndReloadModule(module, MetadataBuilderFlags.PreserveAssemblyReferenceIndices);
            var newAssemblyRefs = GetMembers<AssemblyReference>(newModule, TableIndex.AssemblyRef);
            
            Assert.Equal(originalAssemblyRefs, newAssemblyRefs.Take(originalAssemblyRefs.Count), Comparer);
        }

        [Fact]
        public void PreserveAssemblyRefsWithTypeRefRemovedShouldAtLeastHaveOriginalAssemblyRefs()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_NetCore);
            var originalAssemblyRefs = GetMembers<AssemblyReference>(module, TableIndex.AssemblyRef);
            
            var instructions = module.ManagedEntrypointMethod.CilMethodBody.Instructions;
            instructions.Clear();
            instructions.Add(new CilInstruction(CilOpCodes.Ret));
            
            var newModule = RebuildAndReloadModule(module, MetadataBuilderFlags.PreserveAssemblyReferenceIndices);
            var newAssemblyRefs = GetMembers<AssemblyReference>(newModule, TableIndex.AssemblyRef);
            
            Assert.Equal(originalAssemblyRefs, newAssemblyRefs.Take(originalAssemblyRefs.Count), Comparer);
        }

        [Fact]
        public void PreserveAssemblyRefsWithExtraImportShouldAtLeastHaveOriginalAssemblyRefs()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_NetCore);
            var originalAssemblyRefs = GetMembers<AssemblyReference>(module, TableIndex.AssemblyRef);
            
            var importer = new ReferenceImporter(module);
            var exists = importer.ImportMethod(typeof(File).GetMethod("Exists", new[] {typeof(string)}));

            var instructions = module.ManagedEntrypointMethod.CilMethodBody.Instructions;
            instructions.RemoveAt(instructions.Count-1);
            instructions.AddRange(new[]
            {
                new CilInstruction(CilOpCodes.Ldstr, "file.txt"),
                new CilInstruction(CilOpCodes.Call, exists),
                new CilInstruction(CilOpCodes.Pop),
                new CilInstruction(CilOpCodes.Ret),
            });
            
            var newModule = RebuildAndReloadModule(module, MetadataBuilderFlags.PreserveAssemblyReferenceIndices);
            var newAssemblyRefs = GetMembers<AssemblyReference>(newModule, TableIndex.AssemblyRef);
            
            Assert.Equal(originalAssemblyRefs, newAssemblyRefs.Take(originalAssemblyRefs.Count), Comparer);
        }

        [Fact]
        public void PreserveTypeDefsNoChangeShouldResultInSameTypeDefs()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_NetCore);
            var originalTypeDefs = GetMembers<TypeDefinition>(module, TableIndex.TypeDef);
            
            var newModule = RebuildAndReloadModule(module, MetadataBuilderFlags.PreserveTypeDefinitionIndices);
            var newTypeDefs = GetMembers<TypeDefinition>(newModule, TableIndex.TypeDef);
            
            Assert.Equal(originalTypeDefs, newTypeDefs.Take(originalTypeDefs.Count), Comparer);
        }

        [Fact]
        public void PreserveTypeDefsAddTypeToEndShouldResultInSameTypeDefs()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_NetCore);
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
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_NetCore);
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
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_NetCore);
            for (int i = 0; i < 10; i++)
                module.TopLevelTypes.Add(new TypeDefinition("Namespace", $"Type{i.ToString()}", TypeAttributes.Public));
            module = RebuildAndReloadModule(module, MetadataBuilderFlags.None);

            const int indexToRemove = 2;
            
            var originalTypeDefs = GetMembers<TypeDefinition>(module, TableIndex.TypeDef);
            module.TopLevelTypes.RemoveAt(indexToRemove);
            
            var newModule = RebuildAndReloadModule(module, MetadataBuilderFlags.PreserveTypeDefinitionIndices);
            var newTypeDefs = GetMembers<TypeDefinition>(newModule, TableIndex.TypeDef);

            for (int i = 0; i < originalTypeDefs.Count; i++)
            {
                if (i != indexToRemove)
                    Assert.Equal(originalTypeDefs[i], newTypeDefs[i], Comparer);
            }
        }

        private static ModuleDefinition CreateSampleFieldDefsModule(int typeCount, int fieldsPerType)
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_NetCore);
            
            for (int i = 0; i < typeCount; i++)
            {
                var dummyType = new TypeDefinition("Namespace", $"Type{i.ToString()}",
                    TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed);
                
                module.TopLevelTypes.Add(dummyType);
                for (int j = 0; j < fieldsPerType; j++)
                {
                    dummyType.Fields.Add(new FieldDefinition($"Field{j}",
                        FieldAttributes.Public | FieldAttributes.Static,
                        FieldSignature.CreateStatic(module.CorLibTypeFactory.Int32)));
                }
            }

            return RebuildAndReloadModule(module, MetadataBuilderFlags.None);
        }

        private static void AssertSameTokens(ModuleDefinition module, ModuleDefinition newModule,
            Func<TypeDefinition, IEnumerable<IMemberDefinition>> getMembers, params MetadataToken[] excludeTokens)
        {
            Assert.True(module.TopLevelTypes.Count <= newModule.TopLevelTypes.Count);
            foreach (var originalType in module.TopLevelTypes)
            {
                var newType = newModule.TopLevelTypes.First(t => t.FullName == originalType.FullName);

                var originalMembers = getMembers(originalType).ToArray();
                var newMembers = getMembers(newType).ToArray();
                Assert.True(originalMembers.Length <= newMembers.Length);
                
                foreach (IMemberDefinition originalMember in newMembers)
                {
                    if (originalMember.MetadataToken.Rid == 0 || excludeTokens.Contains(originalMember.MetadataToken))
                        continue;

                    var newMember = newMembers.First(f => f.Name == originalMember.Name);
                    Assert.Equal(originalMember.MetadataToken, newMember.MetadataToken);
                }
            }
        }

        [Fact]
        public void PreserveFieldDefsNoChange()
        {
            var module = CreateSampleFieldDefsModule(10, 10);
            
            var newModule = RebuildAndReloadModule(module,MetadataBuilderFlags.PreserveFieldDefinitionIndices);

            AssertSameTokens(module, newModule, t => t.Fields);
        }

        [Fact]
        public void PreserveFieldDefsChangeOrderOfTypes()
        {
            var module = CreateSampleFieldDefsModule(10, 10);
            
            const int swapIndex = 3;
            var type = module.TopLevelTypes[swapIndex];
            module.TopLevelTypes.RemoveAt(swapIndex);
            module.TopLevelTypes.Insert(swapIndex + 1, type);
            
            var newModule = RebuildAndReloadModule(module,MetadataBuilderFlags.PreserveFieldDefinitionIndices);

            AssertSameTokens(module, newModule, t => t.Fields);
        }

        [Fact]
        public void PreserveFieldDefsChangeOrderOfFieldsInType()
        {
            var module = CreateSampleFieldDefsModule(10, 10);

            const int swapIndex = 3;
            var type = module.TopLevelTypes[2];
            var field = type.Fields[swapIndex];
            type.Fields.RemoveAt(swapIndex);
            type.Fields.Insert(swapIndex + 1, field);
            
            var newModule = RebuildAndReloadModule(module,MetadataBuilderFlags.PreserveFieldDefinitionIndices);

            AssertSameTokens(module, newModule, t => t.Fields);
        }

        [Fact]
        public void PreserveFieldDefsAddExtraField()
        {
            var module = CreateSampleFieldDefsModule(10, 10);

            var type = module.TopLevelTypes[2];
            type.Fields.Insert(3,
                new FieldDefinition("ExtraField", FieldAttributes.Public | FieldAttributes.Static,
                    FieldSignature.CreateStatic(module.CorLibTypeFactory.Int32)));
            
            var newModule = RebuildAndReloadModule(module,MetadataBuilderFlags.PreserveFieldDefinitionIndices);

            AssertSameTokens(module, newModule, t => t.Fields);
        }

        [Fact]
        public void PreserveFieldDefsRemoveField()
        {
            var module = CreateSampleFieldDefsModule(10, 10);

            var type = module.TopLevelTypes[2];
            const int indexToRemove = 3;
            var field = type.Fields[indexToRemove];
            type.Fields.RemoveAt(indexToRemove);
            
            var newModule = RebuildAndReloadModule(module,MetadataBuilderFlags.PreserveFieldDefinitionIndices);

            AssertSameTokens(module, newModule, t => t.Fields, field.MetadataToken);
        }

        private static ModuleDefinition CreateSampleMethodDefsModule(int typeCount, int methodsPerType)
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_NetCore);

            for (int i = 0; i < typeCount; i++)
            {
                var dummyType = new TypeDefinition("Namespace", $"Type{i.ToString()}",
                    TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed);

                module.TopLevelTypes.Add(dummyType);
                for (int j = 0; j < methodsPerType; j++)
                {
                    var method = new MethodDefinition($"Method{j}",
                        MethodAttributes.Public | MethodAttributes.Static,
                        MethodSignature.CreateStatic(module.CorLibTypeFactory.Void));
                    method.CilMethodBody = new CilMethodBody(method);
                    method.CilMethodBody.Instructions.Add(new CilInstruction(CilOpCodes.Ret));
                    dummyType.Methods.Add(method);
                }
            }

            return RebuildAndReloadModule(module, MetadataBuilderFlags.None);
        }
        
        [Fact]
        public void PreserveMethodDefsNoChange()
        {
            var module = CreateSampleMethodDefsModule(10, 10);
            
            var newModule = RebuildAndReloadModule(module,MetadataBuilderFlags.PreserveMethodDefinitionIndices);

            AssertSameTokens(module, newModule, t => t.Methods);
        }

        [Fact]
        public void PreserveMethodDefsChangeOrderOfTypes()
        {
            var module = CreateSampleMethodDefsModule(10, 10);
            
            const int swapIndex = 3;
            var type = module.TopLevelTypes[swapIndex];
            module.TopLevelTypes.RemoveAt(swapIndex);
            module.TopLevelTypes.Insert(swapIndex + 1, type);
            
            var newModule = RebuildAndReloadModule(module,MetadataBuilderFlags.PreserveMethodDefinitionIndices);

            AssertSameTokens(module, newModule, t => t.Methods);
        }

        [Fact]
        public void PreserveMethodDefsChangeOrderOfMethodsInType()
        {
            var module = CreateSampleMethodDefsModule(10, 10);

            const int swapIndex = 3;
            var type = module.TopLevelTypes[2];
            var method = type.Methods[swapIndex];
            type.Methods.RemoveAt(swapIndex);
            type.Methods.Insert(swapIndex + 1, method);
            
            var newModule = RebuildAndReloadModule(module,MetadataBuilderFlags.PreserveMethodDefinitionIndices);

            AssertSameTokens(module, newModule, t => t.Methods);
        }

        [Fact]
        public void PreserveMethodDefsAddExtraMethod()
        {
            var module = CreateSampleMethodDefsModule(10, 10);

            var type = module.TopLevelTypes[2];
            
            var method = new MethodDefinition("ExtraMethod", MethodAttributes.Public | MethodAttributes.Static,
                MethodSignature.CreateStatic(module.CorLibTypeFactory.Void));
            method.CilMethodBody = new CilMethodBody(method)
            {
                Instructions = {new CilInstruction(CilOpCodes.Ret)}
            };
            
            type.Methods.Insert(3, method);
            
            var newModule = RebuildAndReloadModule(module,MetadataBuilderFlags.PreserveMethodDefinitionIndices);

            AssertSameTokens(module, newModule, t => t.Methods);
        }

        [Fact]
        public void PreserveMethodDefsRemoveMethod()
        {
            var module = CreateSampleMethodDefsModule(10, 10);

            var type = module.TopLevelTypes[2];
            const int indexToRemove = 3;
            var method = type.Methods[indexToRemove];
            type.Methods.RemoveAt(indexToRemove);
            
            var newModule = RebuildAndReloadModule(module,MetadataBuilderFlags.PreserveMethodDefinitionIndices);

            AssertSameTokens(module, newModule, m => m.Methods, method.MetadataToken);
        }
        
        private static ModuleDefinition CreateSamplePropertyDefsModule(int typeCount, int propertiesPerType)
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_NetCore);

            for (int i = 0; i < typeCount; i++)
            {
                var dummyType = new TypeDefinition("Namespace", $"Type{i.ToString()}",
                    TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed);

                module.TopLevelTypes.Add(dummyType);
                for (int j = 0; j < propertiesPerType; j++)
                {
                    var property = new PropertyDefinition($"Property{j}", 0,
                        PropertySignature.CreateStatic(module.CorLibTypeFactory.Object));
                    
                    var getMethod = new MethodDefinition($"get_Property{j}", MethodAttributes.Public | MethodAttributes.Static,
                        MethodSignature.CreateStatic(module.CorLibTypeFactory.Object));
                    getMethod.CilMethodBody = new CilMethodBody(getMethod)
                    {
                        Instructions = {new CilInstruction(CilOpCodes.Ldnull), new CilInstruction(CilOpCodes.Ret)}
                    };
                    
                    dummyType.Methods.Add(getMethod);
                    property.Semantics.Add(new MethodSemantics(getMethod, MethodSemanticsAttributes.Getter));
                    dummyType.Properties.Add(property);
                }
            }

            return RebuildAndReloadModule(module, MetadataBuilderFlags.None);
        }
        
        [Fact]
        public void PreservePropertyDefsNoChange()
        {
            var module = CreateSamplePropertyDefsModule(10, 10);
            
            var newModule = RebuildAndReloadModule(module,MetadataBuilderFlags.PreservePropertyDefinitionIndices);

            AssertSameTokens(module, newModule, t => t.Properties);
        }

        [Fact]
        public void PreservePropertyDefsChangeOrderOfTypes()
        {
            var module = CreateSamplePropertyDefsModule(10, 10);
            
            const int swapIndex = 3;
            var type = module.TopLevelTypes[swapIndex];
            module.TopLevelTypes.RemoveAt(swapIndex);
            module.TopLevelTypes.Insert(swapIndex + 1, type);
            
            var newModule = RebuildAndReloadModule(module,MetadataBuilderFlags.PreservePropertyDefinitionIndices);

            AssertSameTokens(module, newModule, t => t.Properties);
        }

        [Fact]
        public void PreservePropertyDefsChangeOrderOfPropertiesInType()
        {
            var module = CreateSamplePropertyDefsModule(10, 10);

            const int swapIndex = 3;
            var type = module.TopLevelTypes[2];
            var property = type.Properties[swapIndex];
            type.Properties.RemoveAt(swapIndex);
            type.Properties.Insert(swapIndex + 1, property);
            
            var newModule = RebuildAndReloadModule(module,MetadataBuilderFlags.PreservePropertyDefinitionIndices);

            AssertSameTokens(module, newModule, t => t.Properties);
        }

        [Fact]
        public void PreservePropertyDefsAddExtraProperty()
        {
            var module = CreateSamplePropertyDefsModule(10, 10);

            var type = module.TopLevelTypes[2];
            
            // Create new property.
            var property = new PropertyDefinition("ExtraProperty", 0,
                PropertySignature.CreateStatic(module.CorLibTypeFactory.Object));
                    
            // Create getter.
            var getMethod = new MethodDefinition("get_ExtraProperty", 
                MethodAttributes.Public | MethodAttributes.Static,
                MethodSignature.CreateStatic(module.CorLibTypeFactory.Object));
            getMethod.CilMethodBody = new CilMethodBody(getMethod)
            {
                Instructions = {new CilInstruction(CilOpCodes.Ldnull), new CilInstruction(CilOpCodes.Ret)}
            };
                    
            // Add new members to type.
            type.Methods.Add(getMethod);
            property.Semantics.Add(new MethodSemantics(getMethod, MethodSemanticsAttributes.Getter));
            type.Properties.Insert(3, property);
            
            // Rebuild and verify.
            var newModule = RebuildAndReloadModule(module,MetadataBuilderFlags.PreservePropertyDefinitionIndices);
            AssertSameTokens(module, newModule, t => t.Properties);
        }

        [Fact]
        public void PreservePropertyDefsRemoveProperty()
        {
            var module = CreateSamplePropertyDefsModule(10, 10);

            var type = module.TopLevelTypes[2];
            const int indexToRemove = 3;
            var Property = type.Properties[indexToRemove];
            type.Properties.RemoveAt(indexToRemove);
            
            var newModule = RebuildAndReloadModule(module,MetadataBuilderFlags.PreservePropertyDefinitionIndices);

            AssertSameTokens(module, newModule, m => m.Properties, Property.MetadataToken);
        }
        
        
        private static ModuleDefinition CreateSampleEventDefsModule(int typeCount, int EventsPerType)
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_NetCore);
            
            var eventHandlerTypeRef = new TypeReference(
                module,
                module.CorLibTypeFactory.CorLibScope,
                "System",
                nameof(EventHandler));

            for (int i = 0; i < typeCount; i++)
            {
                var dummyType = new TypeDefinition("Namespace", $"Type{i.ToString()}",
                    TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed);

                module.TopLevelTypes.Add(dummyType);
                for (int j = 0; j < EventsPerType; j++)
                    AddDummyEventToType(dummyType, eventHandlerTypeRef, $"Event_{j.ToString()}");
            }

            return RebuildAndReloadModule(module, MetadataBuilderFlags.None);
        }

        private static EventDefinition AddDummyEventToType(TypeDefinition dummyType, ITypeDefOrRef eventHandlerTypeRef, string name)
        {
            var eventHandlerTypeSig = eventHandlerTypeRef.ToTypeSignature();
            
            // Define new event.
            var @event = new EventDefinition(name, 0, eventHandlerTypeRef);

            // Create signature for add/remove methods.
            var signature = MethodSignature.CreateStatic(
                eventHandlerTypeRef.Module.CorLibTypeFactory.Void,
                eventHandlerTypeRef.Module.CorLibTypeFactory.Object,
                eventHandlerTypeSig);

            var methodAttributes = MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.SpecialName
                                   | MethodAttributes.HideBySig;

            // Define add.
            var addMethod = new MethodDefinition($"add_{@event.Name}", methodAttributes, signature);
            addMethod.CilMethodBody = new CilMethodBody(addMethod)
            {
                Instructions = {new CilInstruction(CilOpCodes.Ret)}
            };

            // Define remove.
            var removeMethod = new MethodDefinition($"remove_{@event.Name}", methodAttributes, signature);
            removeMethod.CilMethodBody = new CilMethodBody(removeMethod)
            {
                Instructions = {new CilInstruction(CilOpCodes.Ret)}
            };

            // Add members.
            dummyType.Methods.Add(addMethod);
            dummyType.Methods.Add(removeMethod);
            dummyType.Events.Add(@event);

            @event.Semantics.Add(new MethodSemantics(addMethod, MethodSemanticsAttributes.AddOn));
            @event.Semantics.Add(new MethodSemantics(removeMethod, MethodSemanticsAttributes.RemoveOn));

            return @event;
        }

        [Fact]
        public void PreserveEventDefsNoChange()
        {
            var module = CreateSampleEventDefsModule(10, 10);
            
            var newModule = RebuildAndReloadModule(module,MetadataBuilderFlags.PreserveEventDefinitionIndices);

            AssertSameTokens(module, newModule, t => t.Events);
        }

        [Fact]
        public void PreserveEventDefsChangeOrderOfTypes()
        {
            var module = CreateSampleEventDefsModule(10, 10);
            
            const int swapIndex = 3;
            var type = module.TopLevelTypes[swapIndex];
            module.TopLevelTypes.RemoveAt(swapIndex);
            module.TopLevelTypes.Insert(swapIndex + 1, type);
            
            var newModule = RebuildAndReloadModule(module,MetadataBuilderFlags.PreserveEventDefinitionIndices);

            AssertSameTokens(module, newModule, t => t.Events);
        }

        [Fact]
        public void PreserveEventDefsChangeOrderOfEventsInType()
        {
            var module = CreateSampleEventDefsModule(10, 10);

            const int swapIndex = 3;
            var type = module.TopLevelTypes[2];
            var Event = type.Events[swapIndex];
            type.Events.RemoveAt(swapIndex);
            type.Events.Insert(swapIndex + 1, Event);
            
            var newModule = RebuildAndReloadModule(module,MetadataBuilderFlags.PreserveEventDefinitionIndices);

            AssertSameTokens(module, newModule, t => t.Events);
        }

        [Fact]
        public void PreserveEventDefsAddExtraEvent()
        {
            var module = CreateSampleEventDefsModule(10, 10);

            var type = module.TopLevelTypes[2];
            
            // Create new event.
            var eventHandlerTypeRef = new TypeReference(
                module,
                module.CorLibTypeFactory.CorLibScope,
                "System",
                nameof(EventHandler));

            var @event = AddDummyEventToType(type, eventHandlerTypeRef, "ExtraEvent");
            type.Events.Remove(@event);
            type.Events.Insert(3, @event);
            
            // Rebuild and verify.
            var newModule = RebuildAndReloadModule(module,MetadataBuilderFlags.PreserveEventDefinitionIndices);
            AssertSameTokens(module, newModule, t => t.Events);
        }

        [Fact]
        public void PreserveEventDefsRemoveEvent()
        {
            var module = CreateSampleEventDefsModule(10, 10);

            var type = module.TopLevelTypes[2];
            const int indexToRemove = 3;
            var Event = type.Events[indexToRemove];
            type.Events.RemoveAt(indexToRemove);
            
            var newModule = RebuildAndReloadModule(module,MetadataBuilderFlags.PreserveEventDefinitionIndices);

            AssertSameTokens(module, newModule, m => m.Events, Event.MetadataToken);
        }
    }
}