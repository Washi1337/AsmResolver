using System;
using System.Linq;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.TestCases.CustomAttributes;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Xunit;

namespace AsmResolver.Workspaces.DotNet.Tests
{
    public class ReferenceRelationTest : IClassFixture<TestCasesFixture>
    {
        private readonly TestCasesFixture _fixture;

        public ReferenceRelationTest(TestCasesFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void CustomAttributePropertyTest()
        {
            var module = _fixture.CustomAttributesAssembly.ManifestModule;
            var type = module.TopLevelTypes.First(t => t.Name == nameof(CustomAttributesTestClass));
            var method = type.Methods.First(m => m.Name == nameof(CustomAttributesTestClass.NamedInt32Argument));
            var customAttribute = method.CustomAttributes.First();
            var customAttributeNamedArgument = customAttribute.Signature.NamedArguments.First();


            var customAttributeType = module.TopLevelTypes.First(t => t.Name == nameof(TestCaseAttribute));
            var customAttributeProperty = customAttributeType.Properties.First(m => m.Name == nameof(TestCaseAttribute.IntValue));

            var workspace = new DotNetWorkspace();
            workspace.Assemblies.Add(_fixture.CustomAttributesAssembly);
            workspace.Analyze();

            var node = workspace.Index.GetOrCreateNode(customAttributeProperty);
            Assert.Contains(customAttributeNamedArgument,
                node.ForwardRelations.GetObjects(DotNetRelations.ReferenceArgument));
        }

        [Fact]
        public void CustomAttributeFieldTest()
        {
            var module = _fixture.CustomAttributesAssembly.ManifestModule;
            var type = module.TopLevelTypes.First(t => t.Name == nameof(CustomAttributesTestClass));
            var method = type.Methods.First(m => m.Name == nameof(CustomAttributesTestClass.NamedInt32FieldArgument));
            var customAttribute = method.CustomAttributes.First();
            var customAttributeNamedArgument = customAttribute.Signature.NamedArguments.First();


            var customAttributeType = module.TopLevelTypes.First(t => t.Name == nameof(TestCaseAttribute));
            var customAttributeField = customAttributeType.Fields.First(m => m.Name == nameof(TestCaseAttribute.IntFieldValue));

            var workspace = new DotNetWorkspace();
            workspace.Assemblies.Add(_fixture.CustomAttributesAssembly);
            workspace.Analyze();

            var node = workspace.Index.GetOrCreateNode(customAttributeField);
            Assert.Contains(customAttributeNamedArgument,
                node.ForwardRelations.GetObjects(DotNetRelations.ReferenceArgument));
        }

        [Fact]
        public void AssemblyTest()
        {
            var assembly1 = new AssemblyDefinition("Assembly1", new Version(1, 0, 0, 0));
            var module1 = new ModuleDefinition("Assembly1");
            var assembly2 = new AssemblyDefinition("Assembly1", new Version(1, 0, 0, 0));
            var module2 = new ModuleDefinition("Assembly2");

            assembly1.Modules.Add(module1);
            assembly2.Modules.Add(module2);

            var reference = new AssemblyReference(assembly1);
            module2.AssemblyReferences.Add(reference);

            var workspace = new DotNetWorkspace();
            workspace.Assemblies.Add(assembly1);
            workspace.Assemblies.Add(assembly2);

            workspace.Analyze();

            var node = workspace.Index.GetOrCreateNode(assembly1);
            Assert.Contains(reference,
                node.ForwardRelations.GetObjects(DotNetRelations.ReferenceAssembly));
        }

        [Fact]
        public void TypeTest()
        {
            var assembly1 = new AssemblyDefinition("Assembly1", new Version(1, 0, 0, 0));
            var module1 = new ModuleDefinition("Assembly1");
            var assembly2 = new AssemblyDefinition("Assembly1", new Version(1, 0, 0, 0));
            var module2 = new ModuleDefinition("Assembly2");
            var type1 = new TypeDefinition("", "Assembly1", TypeAttributes.Class);
            var type2 = new TypeDefinition("", "Assembly2", TypeAttributes.Class);

            assembly1.Modules.Add(module1);
            assembly2.Modules.Add(module2);

            module1.TopLevelTypes.Add(type1);
            module2.TopLevelTypes.Add(type2);

            var importer = new ReferenceImporter(module2);
            var reference = importer.ImportType(type1);
            type2.BaseType = reference;

            var workspace = new DotNetWorkspace();
            workspace.Assemblies.Add(assembly1);
            workspace.Assemblies.Add(assembly2);

            workspace.Analyze();

            var node = workspace.Index.GetOrCreateNode(type1);
            Assert.Contains(reference,
                node.ForwardRelations.GetObjects(DotNetRelations.ReferenceType));
        }

        [Fact]
        public void ExportedTypeTest()
        {
            var assembly1 = new AssemblyDefinition("Assembly1", new Version(1, 0, 0, 0));
            var module1 = new ModuleDefinition("Assembly1");
            var assembly2 = new AssemblyDefinition("Assembly1", new Version(1, 0, 0, 0));
            var module2 = new ModuleDefinition("Assembly2");

            var type = new TypeDefinition("Namespace", "Type", TypeAttributes.Class);

            assembly1.Modules.Add(module1);
            assembly2.Modules.Add(module2);

            module1.TopLevelTypes.Add(type);

            var assembly1Reference = new AssemblyReference(assembly1);
            var exportedType = new ExportedType(assembly1Reference, "Namespace", "Type");
            module2.ExportedTypes.Add(exportedType);

            var workspace = new DotNetWorkspace();
            workspace.Assemblies.Add(assembly1);
            workspace.Assemblies.Add(assembly2);

            workspace.Analyze();

            var node = workspace.Index.GetOrCreateNode(type);
            Assert.Contains(exportedType,
                node.ForwardRelations.GetObjects(DotNetRelations.ReferenceExportedType));
        }

        [Fact]
        public void MethodTest()
        {
            var assembly1 = new AssemblyDefinition("Assembly1", new Version(1, 0, 0, 0));
            var module1 = new ModuleDefinition("Assembly1");
            var assembly2 = new AssemblyDefinition("Assembly1", new Version(1, 0, 0, 0));
            var module2 = new ModuleDefinition("Assembly2");
            var factory = module2.CorLibTypeFactory;
            var type1 = new TypeDefinition("", "Assembly1", TypeAttributes.Class);
            var type2 = new TypeDefinition("", "Assembly2", TypeAttributes.Class);
            var method1 = new MethodDefinition("Assembly1", MethodAttributes.Static, MethodSignature.CreateInstance(factory.Void));
            var method2 = new MethodDefinition("Assembly2", MethodAttributes.Static, MethodSignature.CreateInstance(factory.Void));

            assembly1.Modules.Add(module1);
            assembly2.Modules.Add(module2);

            module1.TopLevelTypes.Add(type1);
            module2.TopLevelTypes.Add(type2);

            type1.Methods.Add(method1);
            type2.Methods.Add(method2);

            var importer = new ReferenceImporter(module2);
            var reference = importer.ImportMethod(method1);

            var body = new CilMethodBody(method2);
            body.Instructions.Add(new CilInstruction(CilOpCodes.Call, reference));
            method2.MethodBody = body;

            var workspace = new DotNetWorkspace();
            workspace.Assemblies.Add(assembly1);
            workspace.Assemblies.Add(assembly2);

            workspace.Analyze();

            var node = workspace.Index.GetOrCreateNode(method1);
            Assert.Contains(reference,
                node.ForwardRelations.GetObjects(DotNetRelations.ReferenceMember));
        }

        [Fact]
        public void FieldTest()
        {
            var assembly1 = new AssemblyDefinition("Assembly1", new Version(1, 0, 0, 0));
            var module1 = new ModuleDefinition("Assembly1");
            var assembly2 = new AssemblyDefinition("Assembly1", new Version(1, 0, 0, 0));
            var module2 = new ModuleDefinition("Assembly2");
            var factory = module2.CorLibTypeFactory;
            var type1 = new TypeDefinition("", "Assembly1", TypeAttributes.Class);
            var type2 = new TypeDefinition("", "Assembly2", TypeAttributes.Class);
            var method = new MethodDefinition("Assembly2", MethodAttributes.Static, MethodSignature.CreateInstance(factory.Void));
            var field = new FieldDefinition("Assembly1", FieldAttributes.Public,
                FieldSignature.CreateInstance(factory.Boolean));

            assembly1.Modules.Add(module1);
            assembly2.Modules.Add(module2);

            module1.TopLevelTypes.Add(type1);
            module2.TopLevelTypes.Add(type2);

            type2.Methods.Add(method);

            type1.Fields.Add(field);

            var importer = new ReferenceImporter(module2);
            var reference = importer.ImportField(field);

            var body = new CilMethodBody(method);
            body.Instructions.Add(new CilInstruction(CilOpCodes.Call, reference));
            method.MethodBody = body;

            var workspace = new DotNetWorkspace();
            workspace.Assemblies.Add(assembly1);
            workspace.Assemblies.Add(assembly2);

            workspace.Analyze();

            var node = workspace.Index.GetOrCreateNode(field);
            Assert.Contains(reference,
                node.ForwardRelations.GetObjects(DotNetRelations.ReferenceMember));
        }
    }
}
