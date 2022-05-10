using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.DotNet.TestCases.Fields;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Xunit;

namespace AsmResolver.DotNet.Tests
{
    public class ReferenceImporterTest
    {
        private static readonly SignatureComparer Comparer = new();

        private readonly AssemblyReference _dummyAssembly = new("SomeAssembly", new Version(1, 2, 3, 4));
        private readonly ModuleDefinition _module;
        private readonly ReferenceImporter _importer;

        public ReferenceImporterTest()
        {
            _module = new ModuleDefinition("SomeModule.dll");
            _importer = new ReferenceImporter(_module);
        }

        [Fact]
        public void ImportNewAssemblyShouldAddToModule()
        {
            var result = _importer.ImportScope(_dummyAssembly);

            Assert.Equal(_dummyAssembly, result, Comparer);
            Assert.Contains(result, _module.AssemblyReferences);
        }

        [Fact]
        public void ImportExistingAssemblyShouldUseExistingAssembly()
        {
            _module.AssemblyReferences.Add(_dummyAssembly);

            int count = _module.AssemblyReferences.Count;

            var copy = new AssemblyReference(_dummyAssembly);
            var result = _importer.ImportScope(copy);

            Assert.Same(_dummyAssembly, result);
            Assert.Equal(count, _module.AssemblyReferences.Count);
        }

        [Fact]
        public void ImportNewTypeShouldCreateNewReference()
        {
            var type = new TypeReference(_dummyAssembly, "SomeNamespace", "SomeName");
            var result = _importer.ImportType(type);

            Assert.Equal(type, result, Comparer);
            Assert.Equal(_module, result.Module);
        }

        [Fact]
        public void ImportAlreadyImportedTypeShouldUseSameInstance()
        {
            var type = new TypeReference(_dummyAssembly, "SomeNamespace", "SomeName");
            var importedType = _importer.ImportType(type);

            var result = _importer.ImportType(importedType);

            Assert.Same(importedType, result);
        }

        [Fact]
        public void ImportTypeDefFromDifferentModuleShouldReturnTypeRef()
        {
            var assembly = new AssemblyDefinition("ExternalAssembly", new Version(1, 2, 3, 4));
            assembly.Modules.Add(new ModuleDefinition("ExternalAssembly.dll"));
            var definition = new TypeDefinition("SomeNamespace", "SomeName", TypeAttributes.Public);
            assembly.ManifestModule.TopLevelTypes.Add(definition);

            var result = _importer.ImportType(definition);

            Assert.IsAssignableFrom<TypeReference>(result);
            Assert.Equal(definition, result, Comparer);
        }

        [Fact]
        public void ImportTypeDefInSameModuleShouldReturnSameInstance()
        {
            var definition = new TypeDefinition("SomeNamespace", "SomeName", TypeAttributes.Public);
            _module.TopLevelTypes.Add(definition);

            var importedType = _importer.ImportType(definition);

            Assert.Same(definition, importedType);
        }

        [Fact]
        public void ImportNestedTypeShouldImportParentType()
        {
            var declaringType = new TypeReference(_dummyAssembly, "SomeNamespace", "SomeName");
            var nested = new TypeReference(declaringType, null, "Nested");

            var result = _importer.ImportType(nested);

            Assert.Equal(nested, result, Comparer);
            Assert.Equal(_module, result.Module);
            Assert.Equal(_module, result.DeclaringType.Module);
        }

        [Fact]
        public void ImportNestedTypeDefinitionShouldImportParentType()
        {
            var otherAssembly = new AssemblyDefinition(_dummyAssembly.Name, _dummyAssembly.Version);
            var otherModule = new ModuleDefinition("OtherModule");
            otherAssembly.Modules.Add(otherModule);

            var objectType = otherModule.CorLibTypeFactory.Object.ToTypeDefOrRef();

            var declaringType = new TypeDefinition(
                "SomeNamespace",
                "SomeName",
                TypeAttributes.Class | TypeAttributes.Public,
                objectType);
            var nestedType = new TypeDefinition(
                null,
                "NestedType",
                TypeAttributes.Class | TypeAttributes.NestedPublic,
                objectType);

            declaringType.NestedTypes.Add(nestedType);
            otherModule.TopLevelTypes.Add(declaringType);

            var reference = _importer.ImportType(nestedType);

            Assert.NotNull(reference.DeclaringType);
            Assert.Equal(declaringType, reference.DeclaringType, Comparer);
            Assert.Equal(_module, reference.Module);
            Assert.Equal(_module, reference.DeclaringType.Module);
        }

        [Fact]
        public void ImportSimpleTypeFromReflectionShouldResultInTypeRef()
        {
            var type = typeof(Console);

            var result = _importer.ImportType(type);

            Assert.IsAssignableFrom<TypeReference>(result);
            Assert.Equal(type.FullName, result.FullName);
            Assert.Equal(type.Assembly.GetName().Name, result.Scope.Name);
        }

        [Fact]
        public void ImportArrayTypeShouldResultInTypeSpecWithSzArray()
        {
            var type = typeof(Stream[]);

            var result = _importer.ImportType(type);

            Assert.IsAssignableFrom<TypeSpecification>(result);
            Assert.IsAssignableFrom<SzArrayTypeSignature>(((TypeSpecification) result).Signature);
        }

        [Fact]
        public void ImportCorLibTypeAsSignatureShouldResultInCorLibTypeSignature()
        {
            var type = typeof(string[]);

            var result = _importer.ImportType(type);

            Assert.IsAssignableFrom<TypeSpecification>(result);
            var specification = (TypeSpecification) result;
            Assert.IsAssignableFrom<SzArrayTypeSignature>(specification.Signature);
            var arrayType = (SzArrayTypeSignature) specification.Signature;
            Assert.IsAssignableFrom<CorLibTypeSignature>(arrayType.BaseType);
            Assert.Equal(ElementType.String, arrayType.BaseType.ElementType);
        }

        [Fact]
        public void ImportGenericTypeShouldResultInTypeSpecWithGenericInstance()
        {
            var type = typeof(List<string>);

            var result = _importer.ImportType(type);

            Assert.IsAssignableFrom<TypeSpecification>(result);
            var specification = (TypeSpecification) result;
            Assert.IsAssignableFrom<GenericInstanceTypeSignature>(specification.Signature);
            var genericInstance = (GenericInstanceTypeSignature) specification.Signature;
            Assert.Equal(typeof(List<>).FullName, genericInstance.GenericType.FullName);
            Assert.Equal(new TypeSignature[]
            {
                _module.CorLibTypeFactory.String
            }, genericInstance.TypeArguments);
        }

        [Fact]
        public void ImportMethodFromExternalModuleShouldResultInMemberRef()
        {
            var type = new TypeReference(_dummyAssembly, null, "Type");
            var method = new MemberReference(type, "Method",
                MethodSignature.CreateStatic(_module.CorLibTypeFactory.String));

            var result = _importer.ImportMethod(method);

            Assert.Equal(method, result, Comparer);
            Assert.Same(_module, result.Module);
        }

        [Fact]
        public void ImportMethodFromSameModuleShouldResultInSameInstance()
        {
            var type = new TypeDefinition(null, "Type", TypeAttributes.Public);
            _module.TopLevelTypes.Add(type);

            var method = new MethodDefinition("Method", MethodAttributes.Public | MethodAttributes.Static,
                MethodSignature.CreateStatic(_module.CorLibTypeFactory.Void));
            type.Methods.Add(method);

            var result = _importer.ImportMethod(method);

            Assert.Same(method, result);
        }

        [Fact]
        public void ImportMethodFromGenericTypeThroughReflectionShouldIncludeGenericParamSig()
        {
            var method = typeof(List<string>).GetMethod("Add");

            var result = _importer.ImportMethod(method);

            Assert.IsAssignableFrom<GenericParameterSignature>(result.Signature.ParameterTypes[0]);
            var genericParameter = (GenericParameterSignature) result.Signature.ParameterTypes[0];
            Assert.Equal(0, genericParameter.Index);
            Assert.Equal(GenericParameterType.Type, genericParameter.ParameterType);
        }

        [Fact]
        public void ImportGenericMethodFromReflectionShouldResultInMethodSpec()
        {
            var method = typeof(Enumerable)
                .GetMethod("Empty")
                .MakeGenericMethod(typeof(string));

            var result = _importer.ImportMethod(method);

            Assert.IsAssignableFrom<MethodSpecification>(result);
            var specification = (MethodSpecification) result;
            Assert.Equal("Empty", result.Name);
            Assert.Equal(new TypeSignature[]
            {
                _module.CorLibTypeFactory.String
            }, specification.Signature.TypeArguments, Comparer);
        }

        [Fact]
        public void ImportFieldFromExternalModuleShouldResultInMemberRef()
        {
            var type = new TypeReference(_dummyAssembly, null, "Type");
            var field = new MemberReference(
                type,
                "Field",
                new FieldSignature(_module.CorLibTypeFactory.String));

            var result = _importer.ImportField(field);

            Assert.Equal(field, result, Comparer);
            Assert.Same(_module, result.Module);
        }

        [Fact]
        public void ImportFieldFromSameModuleShouldResultInSameInstance()
        {
            var type = new TypeDefinition(null, "Type", TypeAttributes.Public);
            _module.TopLevelTypes.Add(type);

            var field = new FieldDefinition(
                "Field",
                FieldAttributes.Public | FieldAttributes.Static,
                _module.CorLibTypeFactory.Int32);

            type.Fields.Add(field);

            var result = _importer.ImportField(field);

            Assert.Same(field, result);
        }

        [Fact]
        public void ImportFieldFromReflectionShouldResultInMemberRef()
        {
            var field = typeof(string).GetField("Empty");

            var result = _importer.ImportField(field);

            Assert.Equal(field.Name, result.Name);
            Assert.Equal(field.DeclaringType.FullName, result.DeclaringType.FullName);
            Assert.Equal(field.FieldType.FullName, ((FieldSignature) result.Signature).FieldType.FullName);
        }

        [Fact]
        public void ImportNonImportedTypeDefOrRefShouldResultInNewInstance()
        {
            var signature = new TypeReference(_module.CorLibTypeFactory.CorLibScope, "System.IO", "Stream")
                .ToTypeSignature();

            var imported = _importer.ImportTypeSignature(signature);

            Assert.NotSame(signature, imported);
            Assert.Equal(signature, imported, Comparer);
            Assert.Equal(_module, imported.Module);
        }

        [Fact]
        public void ImportTypeSpecWithNonImportedBaseTypeShouldResultInNewInstance()
        {
            var signature = new TypeReference(_module.CorLibTypeFactory.CorLibScope, "System.IO", "Stream")
                .ToTypeSignature()
                .MakeSzArrayType();

            var imported = _importer.ImportTypeSignature(signature);
            var newInstance = Assert.IsAssignableFrom<SzArrayTypeSignature>(imported);
            Assert.NotSame(signature, newInstance);
            Assert.Equal(signature, newInstance, Comparer);
            Assert.Equal(_module, newInstance.BaseType.Module);
        }

        [Fact]
        public void ImportFullyImportedTypeDefOrRefShouldResultInSameInstance()
        {
            var signature = new TypeReference(_module, _module.CorLibTypeFactory.CorLibScope, "System.IO", "Stream")
                .ToTypeSignature();

            var imported = _importer.ImportTypeSignature(signature);
            Assert.Same(signature, imported);
        }

        [Fact]
        public void ImportFullyImportedTypeSpecShouldResultInSameInstance()
        {
            var signature = new TypeReference(_module, _module.CorLibTypeFactory.CorLibScope, "System.IO", "Stream")
                .ToTypeSignature()
                .MakeSzArrayType();

            var imported = _importer.ImportTypeSignature(signature);
            Assert.Same(signature, imported);
        }

        [Fact]
        public void ImportGenericTypeSigWithNonImportedTypeArgumentShouldResultInNewInstance()
        {
            // https://github.com/Washi1337/AsmResolver/issues/268

            var genericType = new TypeDefinition("SomeNamespace", "SomeName", TypeAttributes.Class);
            genericType.GenericParameters.Add(new GenericParameter("T"));
            _module.TopLevelTypes.Add(genericType);

            var instance = genericType.MakeGenericInstanceType(
                new TypeDefOrRefSignature(
                    new TypeReference(_module.CorLibTypeFactory.CorLibScope, "System.IO", "Stream"), false)
            );

            var imported = _importer.ImportTypeSignature(instance);

            var newInstance = Assert.IsAssignableFrom<GenericInstanceTypeSignature>(imported);
            Assert.NotSame(instance, newInstance);
            Assert.Equal(_module, newInstance.Module);
            Assert.Equal(_module, newInstance.TypeArguments[0].Module);
        }

        [Fact]
        public void ImportFullyImportedGenericTypeSigShouldResultInSameInstance()
        {
            // https://github.com/Washi1337/AsmResolver/issues/268

            var genericType = new TypeDefinition("SomeNamespace", "SomeName", TypeAttributes.Class);
            genericType.GenericParameters.Add(new GenericParameter("T"));
            _module.TopLevelTypes.Add(genericType);

            var instance = genericType.MakeGenericInstanceType(
                new TypeDefOrRefSignature(
                    new TypeReference(_module, _module.CorLibTypeFactory.CorLibScope, "System.IO", "Stream"), false)
            );

            var imported = _importer.ImportTypeSignature(instance);

            var newInstance = Assert.IsAssignableFrom<GenericInstanceTypeSignature>(imported);
            Assert.Same(instance, newInstance);
        }

        [Fact]
        public void ImportCustomModifierTypeWithNonImportedModifierTypeShouldResultInNewInstance()
        {
            var signature = new TypeReference(_module, _dummyAssembly, "SomeNamespace", "SomeType")
                .ToTypeSignature()
                .MakeModifierType(new TypeReference(_dummyAssembly, "SomeNamespace", "SomeModifierType"), true);

            var imported = _importer.ImportTypeSignature(signature);

            var newInstance = Assert.IsAssignableFrom<CustomModifierTypeSignature>(imported);
            Assert.NotSame(signature, newInstance);
            Assert.Equal(_module, newInstance.Module);
            Assert.Equal(_module, newInstance.ModifierType.Module);
        }

        [Fact]
        public void ImportFullyImportedCustomModifierTypeShouldResultInSameInstance()
        {
            var signature = new TypeReference(_module, _dummyAssembly, "SomeNamespace", "SomeType")
                .ToTypeSignature()
                .MakeModifierType(new TypeReference(_module, _dummyAssembly, "SomeNamespace", "SomeModifierType"), true);

            var imported = _importer.ImportTypeSignature(signature);

            var newInstance = Assert.IsAssignableFrom<CustomModifierTypeSignature>(imported);
            Assert.Same(signature, newInstance);
        }

        [Fact]
        public void ImportFunctionPointerTypeWithNonImportedParameterShouldResultInNewInstance()
        {
            var signature = MethodSignature
                .CreateStatic(
                    _module.CorLibTypeFactory.Void,
                    new TypeReference(_dummyAssembly, "SomeNamespace", "SomeType").ToTypeSignature())
                .MakeFunctionPointerType();

            var imported = _importer.ImportTypeSignature(signature);

            var newInstance = Assert.IsAssignableFrom<FunctionPointerTypeSignature>(imported);
            Assert.NotSame(signature, newInstance);
            Assert.Equal(signature, newInstance, Comparer);
            Assert.Equal(_module, newInstance.Module);
            Assert.Equal(_module, newInstance.Signature.ParameterTypes[0].Module);
        }

        [Fact]
        public void ImportFunctionPointerTypeWithNonImportedReturnTypeShouldResultInNewInstance()
        {
            var signature = MethodSignature
                .CreateStatic(
                    new TypeReference(_dummyAssembly, "SomeNamespace", "SomeType").ToTypeSignature(),
                    _module.CorLibTypeFactory.Int32)
                .MakeFunctionPointerType();

            var imported = _importer.ImportTypeSignature(signature);

            var newInstance = Assert.IsAssignableFrom<FunctionPointerTypeSignature>(imported);
            Assert.NotSame(signature, newInstance);
            Assert.Equal(signature, newInstance, Comparer);
            Assert.Equal(_module, newInstance.Module);
            Assert.Equal(_module, newInstance.Signature.ReturnType.Module);
        }

        [Fact]
        public void ImportFullyImportedFunctionPointerTypeShouldResultInSameInstance()
        {
            var signature = MethodSignature
                .CreateStatic(
                    _module.CorLibTypeFactory.Void,
                    new TypeReference(_module, _dummyAssembly, "SomeNamespace", "SomeType").ToTypeSignature())
                .MakeFunctionPointerType();

            var imported = _importer.ImportTypeSignature(signature);

            var newInstance = Assert.IsAssignableFrom<FunctionPointerTypeSignature>(imported);
            Assert.Same(signature, newInstance);
        }

        [Fact]
        public void ImportInstanceFieldByReflectionShouldConstructValidFieldSignature()
        {
            // https://github.com/Washi1337/AsmResolver/issues/307

            var module = ModuleDefinition.FromFile(typeof(SingleField).Assembly.Location);
            var field = module.GetAllTypes()
                .First(t => t.Name == nameof(SingleField))
                .Fields
                .First(f => f.Name == nameof(SingleField.IntField));

            var fieldInfo = typeof(SingleField).GetField(nameof(SingleField.IntField))!;

            var importer = new ReferenceImporter(module);
            var imported = importer.ImportField(fieldInfo);
            var resolved = imported.Resolve();

            Assert.NotNull(resolved);
            Assert.Equal(field, Assert.IsAssignableFrom<IFieldDescriptor>(resolved), Comparer);
        }
    }
}
