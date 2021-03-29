using System;
using System.Collections.Generic;
using System.IO;
using AsmResolver.DotNet.Builder;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Xunit;

namespace AsmResolver.DotNet.Tests.Builder
{
    public class TokenMappingTest
    {
        [Fact]
        public void NewTypeDefinition()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);

            // Create new type.
            var type = new TypeDefinition("Namespace", "Name", TypeAttributes.Interface); ;
            module.TopLevelTypes.Add(type);

            // Rebuild.
            var builder = new ManagedPEImageBuilder();
            var result = builder.CreateImage(module);

            // Assert valid token.
            var newToken = result.TokenMapping[type];
            Assert.NotEqual(0u, newToken.Rid);

            // Assert token resolves to the new type.
            var newModule = ModuleDefinition.FromImage(result.ConstructedImage);
            var newType = (TypeDefinition) newModule.LookupMember(newToken);
            Assert.Equal(type.Namespace, newType.Namespace);
            Assert.Equal(type.Name, newType.Name);
        }

        [Fact]
        public void NewFieldDefinition()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);

            // Create new field.
            var field = new FieldDefinition(
                "MyField",
                FieldAttributes.Public | FieldAttributes.Static,
                FieldSignature.CreateStatic(module.CorLibTypeFactory.Object));
            module.GetOrCreateModuleType().Fields.Add(field);

            // Rebuild.
            var builder = new ManagedPEImageBuilder();
            var result = builder.CreateImage(module);

            // Assert valid token.
            var newToken = result.TokenMapping[field];
            Assert.NotEqual(0u, newToken.Rid);

            // Assert token resolves to the new field.
            var newModule = ModuleDefinition.FromImage(result.ConstructedImage);
            var newField = (FieldDefinition) newModule.LookupMember(newToken);
            Assert.Equal(field.Name, newField.Name);
        }

        [Fact]
        public void NewMethodDefinition()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);

            // Create new method.
            var method = new MethodDefinition(
                "MyMethod",
                MethodAttributes.Public | MethodAttributes.Static,
                MethodSignature.CreateStatic(module.CorLibTypeFactory.Void));
            module.GetOrCreateModuleType().Methods.Add(method);

            // Get existing main method.
            var main = module.ManagedEntrypointMethod;

            // Rebuild.
            var builder = new ManagedPEImageBuilder();
            var result = builder.CreateImage(module);

            // Assert valid tokens for both methods.
            var methodToken = result.TokenMapping[method];
            var mainMethodToken = result.TokenMapping[main];
            Assert.NotEqual(0u, methodToken.Rid);
            Assert.NotEqual(0u, mainMethodToken.Rid);

            // Assert tokens resolve to the same methods.
            var newModule = ModuleDefinition.FromImage(result.ConstructedImage);
            var newMethod = (MethodDefinition) newModule.LookupMember(methodToken);
            Assert.Equal(method.Name, newMethod.Name);
            var newMain = (MethodDefinition) newModule.LookupMember(mainMethodToken);
            Assert.Equal(main.Name, newMain.Name);
        }

        [Fact]
        public void NewTypeReference()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);

            // Import arbitrary type as reference.
            var importer = new ReferenceImporter(module);
            var reference = importer.ImportType(typeof(MemoryStream));

            // Ensure type ref is added to the module by adding a dummy field referencing it.
            module.GetOrCreateModuleType().Fields.Add(new FieldDefinition(
                "MyField",
                FieldAttributes.Public | FieldAttributes.Static,
                FieldSignature.CreateStatic(reference.ToTypeSignature())));

            // Rebuild.
            var builder = new ManagedPEImageBuilder();
            var result = builder.CreateImage(module);

            // Assert valid token.
            var newToken = result.TokenMapping[reference];
            Assert.NotEqual(0u, newToken.Rid);

            // Assert token resolves to the same type reference.
            var newModule = ModuleDefinition.FromImage(result.ConstructedImage);
            var newReference = (TypeReference) newModule.LookupMember(newToken);
            Assert.Equal(reference.Namespace, newReference.Namespace);
            Assert.Equal(reference.Name, newReference.Name);
        }

        [Fact]
        public void NewMemberReference()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);

            // Import arbitrary method.
            var importer = new ReferenceImporter(module);
            var reference = importer.ImportMethod(typeof(MemoryStream).GetConstructor(Type.EmptyTypes));

            // Ensure method reference is added to the module by referencing it in main.
            var instructions = module.ManagedEntrypointMethod.CilMethodBody.Instructions;
            instructions.Insert(0, CilOpCodes.Newobj, reference);
            instructions.Insert(1, CilOpCodes.Pop);

            // Rebuild.
            var builder = new ManagedPEImageBuilder();
            var result = builder.CreateImage(module);

            // Assert valid token.
            var newToken = result.TokenMapping[reference];
            Assert.NotEqual(0u, newToken.Rid);

            // Assert token resolves to the same method reference.
            var newModule = ModuleDefinition.FromImage(result.ConstructedImage);
            var newReference = (MemberReference) newModule.LookupMember(newToken);
            Assert.Equal(reference.Name, newReference.Name);
        }

        [Fact]
        public void NewTypeSpecification()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);

            // Import arbitrary generic method.
            var importer = new ReferenceImporter(module);
            var specification = importer.ImportType(typeof(List<object>));

            // Ensure method reference is added to the module by referencing it in main.
            var instructions = module.ManagedEntrypointMethod.CilMethodBody.Instructions;
            instructions.Insert(0, CilOpCodes.Ldtoken, specification);
            instructions.Insert(1, CilOpCodes.Pop);

            // Rebuild.
            var builder = new ManagedPEImageBuilder();
            var result = builder.CreateImage(module);

            // Assert valid token.
            var newToken = result.TokenMapping[specification];
            Assert.NotEqual(0u, newToken.Rid);

            // Assert token resolves to the same method reference.
            var newModule = ModuleDefinition.FromImage(result.ConstructedImage);
            var newReference = (TypeSpecification) newModule.LookupMember(newToken);
            Assert.Equal(specification.Name, newReference.Name);
        }

        [Fact]
        public void NewMethodSpecification()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);

            // Import arbitrary generic method.
            var importer = new ReferenceImporter(module);
            var reference = importer.ImportMethod(typeof(Array).GetMethod("Empty").MakeGenericMethod(typeof(object)));

            // Ensure method reference is added to the module by referencing it in main.
            var instructions = module.ManagedEntrypointMethod.CilMethodBody.Instructions;
            instructions.Insert(0, CilOpCodes.Call, reference);
            instructions.Insert(1, CilOpCodes.Pop);

            // Rebuild.
            var builder = new ManagedPEImageBuilder();
            var result = builder.CreateImage(module);

            // Assert valid token.
            var newToken = result.TokenMapping[reference];
            Assert.NotEqual(0u, newToken.Rid);

            // Assert token resolves to the same method reference.
            var newModule = ModuleDefinition.FromImage(result.ConstructedImage);
            var newReference = (MethodSpecification) newModule.LookupMember(newToken);
            Assert.Equal(reference.Name, newReference.Name);
        }

        [Fact]
        public void NewStandaloneSignature()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);

            // Import arbitrary method signature.
            var importer = new ReferenceImporter(module);
            var signature = new StandAloneSignature(
                importer.ImportMethodSignature(MethodSignature.CreateStatic(module.CorLibTypeFactory.Void)));

            // Ensure reference is added to the module by referencing it in main.
            var instructions = module.ManagedEntrypointMethod.CilMethodBody.Instructions;
            instructions.Insert(0, CilOpCodes.Ldnull);
            instructions.Insert(0, CilOpCodes.Calli, signature);

            // Rebuild.
            var builder = new ManagedPEImageBuilder();
            var result = builder.CreateImage(module);

            // Assert valid token.
            var newToken = result.TokenMapping[signature];
            Assert.NotEqual(0u, newToken.Rid);

            // Assert token resolves to the same method reference.
            var newModule = ModuleDefinition.FromImage(result.ConstructedImage);
            var newSignature = (StandAloneSignature) newModule.LookupMember(newToken);
            Assert.Equal(signature.Signature, newSignature.Signature, new SignatureComparer());
        }
    }
}
