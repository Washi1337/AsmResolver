using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Xunit;

namespace AsmResolver.DotNet.Tests
{
    public class ReferenceImporterTest
    {
        private static readonly SignatureComparer _comparer = new SignatureComparer();

        private readonly AssemblyReference _dummyAssembly = new AssemblyReference("SomeAssembly", new Version(1, 2, 3, 4));
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
            
            Assert.Equal(_dummyAssembly, result, _comparer);
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

            Assert.Equal(type, result, _comparer);
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
            Assert.Equal(definition, result, _comparer);
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

            Assert.Equal(nested, result, _comparer);
            Assert.Equal(_module, result.Module);
            Assert.Equal(_module, result.DeclaringType.Module);
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

            Assert.Equal(method, result, _comparer);
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
            }, specification.Signature.TypeArguments, _comparer);
        }

        [Fact]
        public void ImportFieldFromExternalModuleShouldResultInMemberRef()
        {
            var type = new TypeReference(_dummyAssembly, null, "Type");
            var field = new MemberReference(type, "Field",
                FieldSignature.CreateStatic(_module.CorLibTypeFactory.String));

            var result = _importer.ImportField(field);

            Assert.Equal(field, result, _comparer);
            Assert.Same(_module, result.Module);
        }

        [Fact]
        public void ImportFieldFromSameModuleShouldResultInSameInstance()
        {
            var type = new TypeDefinition(null, "Type", TypeAttributes.Public);
            _module.TopLevelTypes.Add(type);
            
            var field = new FieldDefinition("Method", FieldAttributes.Public | FieldAttributes.Static,
                FieldSignature.CreateStatic(_module.CorLibTypeFactory.Void));
            type.Fields.Add(field);

            var result = _importer.ImportField(field);

            Assert.Same(field, result);
        }
    }
}