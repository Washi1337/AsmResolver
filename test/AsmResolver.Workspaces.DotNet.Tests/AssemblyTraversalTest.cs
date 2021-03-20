using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Metadata.Tables;
using Xunit;

namespace AsmResolver.Workspaces.DotNet.Tests
{
    public class AssemblyTraversalTest : IClassFixture<TestCasesFixture>
    {
        private readonly TestCasesFixture _fixture;

        public AssemblyTraversalTest(TestCasesFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void ModuleCheck()
        {
            var moduleDefinitions = GetAllMembers<ModuleDefinition>(_fixture.AllAssemblies, TableIndex.Module);
            TraverseObjects(moduleDefinitions);
        }

        [Fact]
        public void TypeRefCheck()
        {
            var typeReferences = GetAllMembers<TypeReference>(_fixture.AllAssemblies, TableIndex.TypeRef);
            TraverseObjects(typeReferences);
        }

        [Fact]
        public void TypeDefCheck()
        {
            var typeDefinitions = GetAllMembers<TypeDefinition>(_fixture.AllAssemblies, TableIndex.TypeDef);
            TraverseObjects(typeDefinitions);
        }

        [Fact]
        public void FieldCheck()
        {
            var fieldDefinitions = GetAllMembers<FieldDefinition>(_fixture.AllAssemblies, TableIndex.Field);
            TraverseObjects(fieldDefinitions);
        }

        [Fact]
        public void MethodCheck()
        {
            var methodDefinitions = GetAllMembers<MethodDefinition>(_fixture.AllAssemblies, TableIndex.Method);
            TraverseObjects(methodDefinitions);
        }

        [Fact]
        public void ParamCheck()
        {
            var parameterDefinitions = GetAllMembers<ParameterDefinition>(_fixture.AllAssemblies, TableIndex.Param);
            TraverseObjects(parameterDefinitions);
        }

        [Fact]
        public void InterfaceImplCheck()
        {
            var interfaceImplementations = GetAllMembers<InterfaceImplementation>(_fixture.AllAssemblies, TableIndex.InterfaceImpl);
            TraverseObjects(interfaceImplementations);
        }

        [Fact]
        public void MemberRefCheck()
        {
            var memberReferences = GetAllMembers<MemberReference>(_fixture.AllAssemblies, TableIndex.MemberRef);
            TraverseObjects(memberReferences);
        }

        [Fact]
        public void CustomAttributeCheck()
        {
            var customAttributes = GetAllMembers<CustomAttribute>(_fixture.AllAssemblies, TableIndex.CustomAttribute);
            TraverseObjects(customAttributes);
        }

        [Fact]
        public void DeclSecurityCheck()
        {
            var securityDeclarations = GetAllMembers<SecurityDeclaration>(_fixture.AllAssemblies, TableIndex.DeclSecurity);
            TraverseObjects(securityDeclarations);
        }


        [Fact]
        public void StandAloneSigCheck()
        {
            var standAloneSignatures = GetAllMembers<StandAloneSignature>(_fixture.AllAssemblies, TableIndex.StandAloneSig);
            TraverseObjects(standAloneSignatures);
        }


        [Fact]
        public void EventCheck()
        {
            var eventDefinitions = GetAllMembers<EventDefinition>(_fixture.AllAssemblies, TableIndex.Event);
            TraverseObjects(eventDefinitions);
        }

        [Fact]
        public void PropertyCheck()
        {
            var propertyDefinitions = GetAllMembers<PropertyDefinition>(_fixture.AllAssemblies, TableIndex.Property);
            TraverseObjects(propertyDefinitions);
        }

        [Fact]
        public void ModuleRefCheck()
        {
            var moduleReferences = GetAllMembers<ModuleReference>(_fixture.AllAssemblies, TableIndex.ModuleRef);
            TraverseObjects(moduleReferences);
        }

        [Fact]
        public void TypeSpecCheck()
        {
            var typeSpecifications = GetAllMembers<TypeSpecification>(_fixture.AllAssemblies, TableIndex.TypeSpec);
            TraverseObjects(typeSpecifications);
        }

        [Fact]
        public void AssemblyCheck()
        {
            var assemblyDefinitions = GetAllMembers<AssemblyDefinition>(_fixture.AllAssemblies, TableIndex.Assembly);
            TraverseObjects(assemblyDefinitions);
        }

        [Fact]
        public void FileCheck()
        {
            var fileReferences = GetAllMembers<FileReference>(_fixture.AllAssemblies, TableIndex.File);
            TraverseObjects(fileReferences);
        }

        [Fact]
        public void ExportedTypeCheck()
        {
            var exportedTypes = GetAllMembers<ExportedType>(_fixture.AllAssemblies, TableIndex.ExportedType);
            TraverseObjects(exportedTypes);
        }

        [Fact]
        public void ManifestResourceCheck()
        {
            var manifestResources = GetAllMembers<ManifestResource>(_fixture.AllAssemblies, TableIndex.ManifestResource);
            TraverseObjects(manifestResources);
        }

        [Fact]
        public void GenericParamCheck()
        {
            var genericParameters = GetAllMembers<GenericParameter>(_fixture.AllAssemblies, TableIndex.GenericParam);
            TraverseObjects(genericParameters);
        }

        [Fact]
        public void MethodSpecCheck()
        {
            var methodSpecifications = GetAllMembers<MethodSpecification>(_fixture.AllAssemblies, TableIndex.MethodSpec);
            TraverseObjects(methodSpecifications);
        }

        [Fact]
        public void GenericParamConstraintCheck()
        {
            var genericParameterConstraints =
                GetAllMembers<GenericParameterConstraint>(_fixture.AllAssemblies, TableIndex.GenericParamConstraint);
            TraverseObjects(genericParameterConstraints);
        }

        private void TraverseObjects(IEnumerable<object> members)
        {
            Assert.NotEmpty(members);

            var workspace = new DotNetWorkspace();
            var context = new AnalysisContext(workspace);

            foreach (var assembly in _fixture.AllAssemblies)
            {
                workspace.Assemblies.Add(assembly);
                context.SchedulaForAnalysis(assembly);
            }

            workspace.Analyze(context);

            Assert.All(members, member
                => Assert.True(context.TraversedObjects.Contains(member)));
        }


        public static IEnumerable<T> GetAllMembers<T>(IEnumerable<AssemblyDefinition> assemblies, TableIndex table)
            where T : IMetadataMember
            => assemblies.SelectMany(a => GetMembers<T>(a, table));

        public static IEnumerable<T> GetMembers<T>(AssemblyDefinition assembly, TableIndex table)
            where T : IMetadataMember
        {
            var module = assembly.ManifestModule;
            var tableStream = module.DotNetDirectory.Metadata.GetStream<TablesStream>();
            int size = tableStream.GetTable(table).Count;
            if (size == 0)
                yield break;
            for (uint i = 1; i <= size; i++)
            {
                var member = module.LookupMember(new MetadataToken(table, i));
                if (member is not T value)
                    continue;
                yield return value;
            }
        }

/* TODO: Check those
FieldPtr = 3,
Constant
MethodPtr = 5,
ParamPtr = 7,
FieldMarshal = 13,
ClassLayout = 15,
FieldLayout = 16,
EventMap = 18,
EventPtr = 19,
PropertyMap = 21,
PropertyPtr = 22,
MethodSemantics = 24,
MethodImpl = 25,
NestedClass
ImplMap = 28,
FieldRva = 29,
EncLog = 30,
EncMap = 31,
AssemblyProcessor = 33,
AssemblyOS = 34,
AssemblyRef = 35,
AssemblyRefProcessor = 36,
AssemblyRefOS = 37,
*/
    }
}
