using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.TestCases.Events;
using AsmResolver.DotNet.TestCases.Methods;
using AsmResolver.DotNet.TestCases.Properties;
using AsmResolver.PE.DotNet;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using AsmResolver.PE.DotNet.VTableFixups;
using AsmResolver.PE.File.Headers;
using AsmResolver.Tests.Runners;
using Xunit;

namespace AsmResolver.DotNet.Tests
{
    public class MethodDefinitionTest: IClassFixture<TemporaryDirectoryFixture>
    {
        private const string NonWindowsPlatform = "Test operates on native Windows binaries.";

        private readonly TemporaryDirectoryFixture _fixture;

        public MethodDefinitionTest(TemporaryDirectoryFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void ReadName()
        {
            var module = ModuleDefinition.FromFile(typeof(SingleMethod).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(SingleMethod));
            var method = type.Methods.FirstOrDefault(m => m.Name == nameof(SingleMethod.VoidParameterlessMethod));
            Assert.NotNull(method);
        }

        [Theory]
        [InlineData(".ctor", "System.Void")]
        [InlineData(nameof(MultipleMethods.VoidParameterlessMethod), "System.Void")]
        [InlineData(nameof(MultipleMethods.IntParameterlessMethod), "System.Int32")]
        [InlineData(nameof(MultipleMethods.TypeDefOrRefParameterlessMethod), "AsmResolver.DotNet.TestCases.Methods.MultipleMethods")]
        public void ReadReturnType(string methodName, string expectedReturnType)
        {
            var module = ModuleDefinition.FromFile(typeof(MultipleMethods).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(MultipleMethods));
            var method = type.Methods.First(m => m.Name == methodName);
            Assert.Equal(expectedReturnType, method.Signature.ReturnType.FullName);
        }

        [Fact]
        public void ReadDeclaringType()
        {
            var module = ModuleDefinition.FromFile(typeof(SingleMethod).Assembly.Location);
            var method = (MethodDefinition) module.LookupMember(
                typeof(SingleMethod).GetMethod(nameof(SingleMethod.VoidParameterlessMethod)).MetadataToken);
            Assert.NotNull(method.DeclaringType);
            Assert.Equal(nameof(SingleMethod), method.DeclaringType.Name);
        }

        [Fact]
        public void ReadEmptyParameterDefinitions()
        {
            var module = ModuleDefinition.FromFile(typeof(MultipleMethods).Assembly.Location);
            var method = (MethodDefinition) module.LookupMember(
                typeof(MultipleMethods).GetMethod(nameof(MultipleMethods.VoidParameterlessMethod)).MetadataToken);
            Assert.Empty(method.ParameterDefinitions);
        }

        [Fact]
        public void ReadSingleParameterDefinition()
        {
            var module = ModuleDefinition.FromFile(typeof(MultipleMethods).Assembly.Location);
            var method = (MethodDefinition) module.LookupMember(
                typeof(MultipleMethods).GetMethod(nameof(MultipleMethods.SingleParameterMethod)).MetadataToken);
            Assert.Single(method.ParameterDefinitions);
        }

        [Fact]
        public void ReadMultipleParameterDefinitions()
        {
            var module = ModuleDefinition.FromFile(typeof(MultipleMethods).Assembly.Location);
            var method = (MethodDefinition) module.LookupMember(
                typeof(MultipleMethods).GetMethod(nameof(MultipleMethods.MultipleParameterMethod)).MetadataToken);
            Assert.Equal(3, method.ParameterDefinitions.Count);
        }

        [Fact]
        public void ReadEmptyParametersStatic()
        {
            var module = ModuleDefinition.FromFile(typeof(MultipleMethods).Assembly.Location);
            var method = (MethodDefinition) module.LookupMember(
                typeof(MultipleMethods).GetMethod(nameof(MultipleMethods.VoidParameterlessMethod)).MetadataToken);
            Assert.Empty(method.Parameters);
        }

        [Fact]
        public void ReadSingleParameterStatic()
        {
            var module = ModuleDefinition.FromFile(typeof(MultipleMethods).Assembly.Location);
            var method = (MethodDefinition) module.LookupMember(
                typeof(MultipleMethods).GetMethod(nameof(MultipleMethods.SingleParameterMethod)).MetadataToken);
            Assert.Single(method.Parameters);
            Assert.Equal("intParameter", method.Parameters[0].Name);
            Assert.True(method.Parameters[0].ParameterType.IsTypeOf("System", "Int32"));
        }

        [Fact]
        public void ReadMultipleParameterStatic()
        {
            var module = ModuleDefinition.FromFile(typeof(MultipleMethods).Assembly.Location);
            var method = (MethodDefinition) module.LookupMember(
                typeof(MultipleMethods).GetMethod(nameof(MultipleMethods.MultipleParameterMethod)).MetadataToken);
            Assert.Equal(3, method.Parameters.Count);

            Assert.Equal("intParameter", method.Parameters[0].Name);
            Assert.True(method.Parameters[0].ParameterType.IsTypeOf("System", "Int32"),
                "Expected first parameter to be of type System.Int32.");

            Assert.Equal("stringParameter", method.Parameters[1].Name);
            Assert.True(method.Parameters[1].ParameterType.IsTypeOf("System", "String"),
                "Expected second parameter to be of type System.String.");

            Assert.Equal("typeDefOrRefParameter", method.Parameters[2].Name);
            Assert.True(method.Parameters[2].ParameterType.IsTypeOf("AsmResolver.DotNet.TestCases.Methods", "MultipleMethods"),
                "Expected third parameter to be of type AsmResolver.TestCases.DotNet.MultipleMethods.");
        }

        [Fact]
        public void ReadNormalMethod()
        {
            var module = ModuleDefinition.FromFile(typeof(SingleMethod).Assembly.Location);
            var method = (MethodDefinition) module.LookupMember(
                typeof(SingleMethod).GetMethod(nameof(SingleMethod.VoidParameterlessMethod)).MetadataToken);

            Assert.False(method.IsGetMethod);
            Assert.False(method.IsSetMethod);
            Assert.False(method.IsAddMethod);
            Assert.False(method.IsRemoveMethod);
            Assert.False(method.IsFireMethod);
        }

        [Fact]
        public void ReadIsGetMethod()
        {
            var module = ModuleDefinition.FromFile(typeof(SingleProperty).Assembly.Location);
            var method = (MethodDefinition) module.LookupMember(
                typeof(SingleProperty).GetMethod("get_" + nameof(SingleProperty.IntProperty)).MetadataToken);

            Assert.True(method.IsGetMethod);
        }

        [Fact]
        public void ReadIsSetMethod()
        {
            var module = ModuleDefinition.FromFile(typeof(SingleProperty).Assembly.Location);
            var method = (MethodDefinition) module.LookupMember(
                typeof(SingleProperty).GetMethod("set_" + nameof(SingleProperty.IntProperty)).MetadataToken);

            Assert.True(method.IsSetMethod);
        }

        [Fact]
        public void ReadIsAddMethod()
        {
            var module = ModuleDefinition.FromFile(typeof(SingleEvent).Assembly.Location);
            var method = (MethodDefinition) module.LookupMember(
                typeof(SingleEvent).GetMethod("add_" + nameof(SingleEvent.SimpleEvent)).MetadataToken);

            Assert.True(method.IsAddMethod);
        }

        [Fact]
        public void ReadIsRemoveMethod()
        {
            var module = ModuleDefinition.FromFile(typeof(SingleEvent).Assembly.Location);
            var method = (MethodDefinition) module.LookupMember(
                typeof(SingleEvent).GetMethod("remove_" + nameof(SingleEvent.SimpleEvent)).MetadataToken);

            Assert.True(method.IsRemoveMethod);
        }

        [Fact]
        public void ReadSignatureIsReturnsValue()
        {
            var module = ModuleDefinition.FromFile(typeof(MultipleMethods).Assembly.Location);
            var method = module.TopLevelTypes
                .First(t => t.Name == nameof(MultipleMethods)).Methods
                .First(m => m.Name == nameof(MultipleMethods.IntParameterlessMethod));
            Assert.True(method.Signature.ReturnsValue);
        }

        [Fact]
        public void ReadSignatureNotReturnsValue()
        {
            var module = ModuleDefinition.FromFile(typeof(MultipleMethods).Assembly.Location);
            var method = module.TopLevelTypes
                .First(t => t.Name == nameof(MultipleMethods)).Methods
                .First(m => m.Name == nameof(MultipleMethods.VoidParameterlessMethod));
            Assert.False(method.Signature.ReturnsValue);
        }

        private static AssemblyDefinition CreateDummyLibraryWithExport(int methodCount, bool is32Bit)
        {
            // Build new dummy lib.
            var assembly = new AssemblyDefinition("MyLibrary", new Version(1, 0, 0, 0));
            var module = new ModuleDefinition("MyLibrary.dll");
            module.Attributes &= ~DotNetDirectoryFlags.ILOnly;

            if (is32Bit)
            {
                module.Attributes |= DotNetDirectoryFlags.Bit32Required;
                module.FileCharacteristics |= Characteristics.Dll | Characteristics.Machine32Bit;
                module.PEKind = OptionalHeaderMagic.Pe32;
                module.MachineType = MachineType.I386;
            }
            else
            {
                module.FileCharacteristics |= Characteristics.Dll;
                module.PEKind = OptionalHeaderMagic.Pe32Plus;
                module.MachineType = MachineType.Amd64;
            }

            assembly.Modules.Add(module);

            // Import Console.WriteLine.
            var importer = new ReferenceImporter(module);
            var writeLine = importer.ImportMethod(new MemberReference(
                new TypeReference(module.CorLibTypeFactory.CorLibScope, "System", "Console"),
                "WriteLine",
                MethodSignature.CreateStatic(module.CorLibTypeFactory.Void, module.CorLibTypeFactory.String)));

            // Add a couple unmanaged exports.
            for (int i = 0; i < methodCount; i++)
                AddExportedMethod($"MyMethod{i.ToString()}");

            return assembly;

            void AddExportedMethod(string methodName)
            {
                // New static method.
                var method = new MethodDefinition(
                    methodName,
                    MethodAttributes.Public | MethodAttributes.Static,
                    MethodSignature.CreateStatic(module.CorLibTypeFactory.Void));

                // Add a Console.WriteLine call
                var body = new CilMethodBody(method);
                method.MethodBody = body;
                body.Instructions.Add(CilOpCodes.Ldstr, $"Hello from {methodName}.");
                body.Instructions.Add(CilOpCodes.Call, writeLine);
                body.Instructions.Add(CilOpCodes.Ret);

                // Add to <Module>
                module.GetOrCreateModuleType().Methods.Add(method);

                // Register as unmanaged export.
                method.ExportInfo = new UnmanagedExportInfo(methodName,
                    is32Bit ? VTableType.VTable32Bit : VTableType.VTable64Bit);
            }
        }

        [SkippableTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void ExportMethodByName(bool is32Bit)
        {
            Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.Windows), NonWindowsPlatform);

            const int methodCount = 3;

            var assembly = CreateDummyLibraryWithExport(methodCount, is32Bit);
            var runner = _fixture.GetRunner<NativePERunner>();

            string suffix = is32Bit ? "x86" : "x64";

            // Write library to temp dir.
            string libraryPath = Path.ChangeExtension(runner.GetTestExecutablePath(
                nameof(MethodDefinitionTest),
                nameof(ExportMethodByName),
                $"MyLibrary.{suffix}"), ".dll");
            assembly.Write(libraryPath);

            // Write caller to temp dir.
            string callerPath = runner.GetTestExecutablePath(
                nameof(MethodDefinitionTest),
                nameof(ExportMethodByName),
                $"CallManagedExport.{suffix}");
            File.WriteAllBytes(callerPath, is32Bit
                ? Properties.Resources.CallManagedExport_X86
                : Properties.Resources.CallManagedExport_X64);

            // Verify all exported methods.
            for (int i = 0; i < methodCount; i++)
            {
                string methodName = $"MyMethod{i.ToString()}";
                string output = runner.RunAndCaptureOutput(callerPath, new[]
                {
                    libraryPath,
                    methodName
                });
                Assert.Contains($"Hello from {methodName}.", output);
            }
        }

    }
}
