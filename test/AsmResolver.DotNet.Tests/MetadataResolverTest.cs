using System.IO;
using System.Linq;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.TestCases.NestedClasses;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables;
using Xunit;

namespace AsmResolver.DotNet.Tests
{
    public class MetadataResolverTest
    {
        private readonly RuntimeContext _fwContext;
        private readonly RuntimeContext _coreContext;
        private readonly SignatureComparer _fwComparer;
        private readonly SignatureComparer _coreComparer;

        public MetadataResolverTest()
        {
            _fwContext = new RuntimeContext(
                DotNetRuntimeInfo.NetFramework(4, 0),
                searchDirectories: [Path.GetDirectoryName(typeof(MetadataResolverTest).Assembly.Location)!]
            );

            _coreContext = new RuntimeContext(
                DotNetRuntimeInfo.NetCoreApp(3, 1, 0),
                searchDirectories: [Path.GetDirectoryName(typeof(MetadataResolverTest).Assembly.Location)!]
            );

            _fwComparer = new SignatureComparer(_fwContext);
            _coreComparer = new SignatureComparer(_coreContext);
        }

        [Fact]
        public void ResolveSystemObjectFramework()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters);

            var reference = module.CorLibTypeFactory.CorLibScope.CreateTypeReference("System", "Object");
            var definition = reference.Resolve(_fwContext);

            Assert.Equal((ITypeDefOrRef) reference, definition, _fwComparer);
        }

        [Fact]
        public void ResolveSystemObjectNetCore()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_NetCore, TestReaderParameters);

            var reference = module.CorLibTypeFactory.CorLibScope.CreateTypeReference("System", "Object");
            var definition = reference.Resolve(_coreContext);

            Assert.NotNull(definition);
            Assert.True(definition.IsTypeOf(reference.Namespace, reference.Name));
        }

        [Fact]
        public void ResolveCorLibTypeSignature()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters);
            var definition = module.CorLibTypeFactory.Object.Resolve(_fwContext);

            Assert.Equal(module.CorLibTypeFactory.Object.Type, definition, _fwComparer);
        }

        [Fact]
        public void ResolveType()
        {
            var module = ModuleDefinition.FromFile(typeof(TopLevelClass1).Assembly.Location, TestReaderParameters);

            var topLevelClass1 = module.Assembly!.ToAssemblyReference()
                .CreateTypeReference(typeof(TopLevelClass1).Namespace, nameof(TopLevelClass1));

            var definition = topLevelClass1.Resolve(_coreContext);
            Assert.Equal((ITypeDefOrRef) topLevelClass1, definition, _fwComparer);
        }

        [Fact]
        public void ResolveTypeReferenceTwice()
        {
            var module = new ModuleDefinition("SomeModule.dll");

            var consoleType = module.CorLibTypeFactory.CorLibScope.CreateTypeReference("System", "Console");
            Assert.Same(consoleType.Resolve(_fwContext), consoleType.Resolve(_fwContext));
        }

        [Fact]
        public void ResolveTypeReferenceThenChangeRefAndResolveAgain()
        {
            var module = new ModuleDefinition("SomeModule.dll");

            ITypeDefOrRef expected = module.CorLibTypeFactory.CorLibScope.CreateTypeReference("System", "Object");
            var reference = module.CorLibTypeFactory.CorLibScope.CreateTypeReference("System", "Object");
            Assert.Equal(expected, reference.Resolve(_fwContext), _fwComparer);
            reference.Name = "String";
            Assert.NotEqual(expected, reference.Resolve(_fwContext), _fwComparer);
        }

        [Fact]
        public void ResolveTypeReferenceThenChangeDefAndResolveAgain()
        {
            var module = new ModuleDefinition("SomeModule.dll");

            ITypeDefOrRef expected = module.CorLibTypeFactory.CorLibScope.CreateTypeReference("System", "Object");
            var reference = module.CorLibTypeFactory.CorLibScope.CreateTypeReference("System", "Object");

            var definition = reference.Resolve(_fwContext);
            Assert.Equal(expected, definition, _fwComparer);

            definition.Name = "Foo";
            Assert.False(reference.TryResolve(_fwContext, out _));
        }

        [Fact]
        public void ResolveNestedType()
        {
            var module = ModuleDefinition.FromFile(typeof(TopLevelClass1).Assembly.Location, TestReaderParameters);

            var topLevelClass1 = module.Assembly!.ToAssemblyReference()
                .CreateTypeReference(typeof(TopLevelClass1).Namespace, nameof(TopLevelClass1));
            var nested1 = topLevelClass1.CreateTypeReference(nameof(TopLevelClass1.Nested1));

            var definition = nested1.Resolve(_coreContext);

            Assert.Equal((ITypeDefOrRef) nested1, definition, _coreComparer);
        }

        [Fact]
        public void ResolveNestedNestedType()
        {
            var module = ModuleDefinition.FromFile(typeof(TopLevelClass1).Assembly.Location, TestReaderParameters);

            var topLevelClass1 = module.Assembly!.ToAssemblyReference()
                .CreateTypeReference(typeof(TopLevelClass1).Namespace, nameof(TopLevelClass1));
            var nested1 = topLevelClass1.CreateTypeReference(nameof(TopLevelClass1.Nested1));
            var nested1nested1 = nested1.CreateTypeReference(nameof(TopLevelClass1.Nested1.Nested1Nested1));

            var definition = nested1nested1.Resolve(_fwContext);

            Assert.Equal((ITypeDefOrRef) nested1nested1, definition, _fwComparer);
        }

        [Fact]
        public void ResolveTypeWithSelfAssemblyScope()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters);
            var original = module.TopLevelTypes.First(t => t.Name == "Program");
            var reference = module.TopLevelTypes.First(t => t.Name == "Program").ToTypeReference();

            var definition = reference.Resolve(module.RuntimeContext);

            Assert.Same(original, definition);
        }

        [Fact]
        public void ResolveTypeWithSelfAssemblyScopeNoContext()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters);
            var original = module.TopLevelTypes.First(t => t.Name == "Program");
            var reference = module.Assembly!.ToAssemblyReference().CreateTypeReference(original.Namespace, original.Name!);

            var definition = reference.Resolve(module.RuntimeContext);

            Assert.Same(original, definition);
        }

        [Fact]
        public void ResolveTypeWithModuleScope()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.TypeRefModuleScope, TestReaderParameters);
            var reference = module.LookupMember<TypeReference>(new MetadataToken(TableIndex.TypeRef, 2));

            var definition = reference.Resolve(module.RuntimeContext);

            Assert.NotNull(definition);
            Assert.Same(module, definition.DeclaringModule);
        }

        [Fact]
        public void ResolveTypeWithNullScopeCurrentModule()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.TypeRefNullScope_CurrentModule, TestReaderParameters);
            var reference = module.LookupMember<TypeReference>(new MetadataToken(TableIndex.TypeRef, 2));

            var definition = reference.Resolve(module.RuntimeContext);

            Assert.NotNull(definition);
            Assert.Same(module, definition.DeclaringModule);
        }

        [Fact]
        public void ResolveTypeWithNullScopeExportedType()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.TypeRefNullScope_ExportedType, TestReaderParameters);
            var reference = module.LookupMember<TypeReference>(new MetadataToken(TableIndex.TypeRef, 1));

            var definition = reference.Resolve(module.RuntimeContext);

            Assert.NotNull(definition);
            Assert.Equal("mscorlib", definition.DeclaringModule!.Assembly!.Name);
        }

        [Fact]
        public void ResolveConsoleWriteLineMethod()
        {
            var module = new ModuleDefinition("SomeModule.dll");

            var consoleType = module.CorLibTypeFactory.CorLibScope.CreateTypeReference("System", "Console");
            var writeLineMethod = consoleType.CreateMethodReference(
                "WriteLine",
                MethodSignature.CreateStatic(module.CorLibTypeFactory.Void, [module.CorLibTypeFactory.String])
            );

            var definition = writeLineMethod.Resolve(_fwContext);

            Assert.Equal(writeLineMethod.Name, definition.Name);
            Assert.Equal(writeLineMethod.Signature, definition.Signature, _fwComparer!);
        }

        [Fact]
        public void ResolveStringEmptyField()
        {
            var module = new ModuleDefinition("SomeModule.dll");

            var stringType = module.CorLibTypeFactory.CorLibScope.CreateTypeReference("System", "String");
            var emptyField = stringType.CreateFieldReference("Empty", module.CorLibTypeFactory.String);

            var definition = emptyField.Resolve(_fwContext);

            Assert.Equal(emptyField.Name, definition.Name);
            Assert.Equal(emptyField.Signature, definition.Signature, _fwComparer!);
        }

        [Fact]
        public void ResolveExportedMemberReference()
        {
            // Issue: https://github.com/Washi1337/AsmResolver/issues/124

            // Load main assembly.
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_Forwarder, TestReaderParameters);

            // Load dependencies.
            var context = module.RuntimeContext!;
            context.LoadAssembly(Properties.Resources.Assembly1_Forwarder);
            context.LoadAssembly(Properties.Resources.Assembly2_Actual);

            // Resolve
            var instructions = module.ManagedEntryPointMethod!.CilMethodBody!.Instructions;
            Assert.True(((IMethodDescriptor) instructions[0].Operand!).TryResolve(module.RuntimeContext, out _));
        }

        [Fact]
        public void MaliciousExportedTypeLoop()
        {
            // Load main assembly.
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_MaliciousExportedTypeLoop, TestReaderParameters);

            var context = module.RuntimeContext!;
            context.LoadAssembly(Properties.Resources.Assembly1_MaliciousExportedTypeLoop);
            context.LoadAssembly(Properties.Resources.Assembly2_MaliciousExportedTypeLoop);

            // Find reference to exported type loop.
            var reference = module
                .GetImportedTypeReferences()
                .First(t => t.Name == "SomeName");

            // Attempt to resolve. The test here is that it should not result in an infinite loop / stack overflow.
            Assert.False(reference.TryResolve(module.RuntimeContext, out _));
        }

        [Fact]
        public void ResolveToOlderNetVersion()
        {
            // https://github.com/Washi1337/AsmResolver/issues/321

            var mainApp = ModuleDefinition.FromBytes(Properties.Resources.DifferentNetVersion_MainApp, TestReaderParameters);
            var context = mainApp.RuntimeContext!;

            var library = context.LoadAssembly(Properties.Resources.DifferentNetVersion_Library).ManifestModule!;

            var definition = library
                .TopLevelTypes.First(t => t.Name == "MyClass")
                .Methods.First(m => m.Name == "ThrowMe");

            var reference = (IMethodDescriptor) mainApp.ManagedEntryPointMethod!.CilMethodBody!.Instructions.First(
                    i => i.OpCode == CilOpCodes.Callvirt && ((IMethodDescriptor?) i.Operand)?.Name == "ThrowMe")
                .Operand!;

            var resolved = reference.Resolve(mainApp.RuntimeContext);
            Assert.Equal(definition, resolved);
        }

        [Fact]
        public void ResolveMethodWithoutHideBySig()
        {
            // https://github.com/Washi1337/AsmResolver/issues/241

            var classLibrary = ModuleDefinition.FromFile(typeof(ClassLibraryVB.Class1).Assembly.Location, TestReaderParameters);
            var definitions = classLibrary
                .TopLevelTypes.First(t => t.Name == nameof(ClassLibraryVB.Class1))
                .Methods.Where(m => m.Name == nameof(ClassLibraryVB.Class1.Test))
                .OrderBy(x => x.Parameters.Count)
                .ToArray();

            var helloWorld = ModuleDefinition.FromFile(typeof(HelloWorldVB.Program).Assembly.Location, TestReaderParameters);
            var resolved = helloWorld.ManagedEntryPointMethod!.CilMethodBody!.Instructions
                .Where(x => x.OpCode == CilOpCodes.Call)
                .Select(x => ((IMethodDescriptor) x.Operand!).Resolve(classLibrary.RuntimeContext))
                .ToArray();

            Assert.Equal(definitions, resolved, new SignatureComparer());
        }

    }
}
