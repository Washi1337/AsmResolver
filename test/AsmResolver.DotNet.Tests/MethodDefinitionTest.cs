using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.TestCases.Events;
using AsmResolver.DotNet.TestCases.Generics;
using AsmResolver.DotNet.TestCases.Methods;
using AsmResolver.DotNet.TestCases.Properties;
using AsmResolver.PE.DotNet;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.VTableFixups;
using AsmResolver.PE.Exports;
using AsmResolver.PE.File;
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
        public void ReadParameterlessInt32MethodFullName()
        {
            var module = ModuleDefinition.FromFile(typeof(MultipleMethods).Assembly.Location);
            var method = (MethodDefinition) module.LookupMember(
                typeof(MultipleMethods).GetMethod(nameof(MultipleMethods.IntParameterlessMethod)).MetadataToken);

            Assert.Equal(
                "System.Int32 AsmResolver.DotNet.TestCases.Methods.MultipleMethods::IntParameterlessMethod()",
                method.FullName);
        }

        [Fact]
        public void ReadParameterlessMethodFullName()
        {
            var module = ModuleDefinition.FromFile(typeof(MultipleMethods).Assembly.Location);
            var method = (MethodDefinition) module.LookupMember(
                typeof(MultipleMethods).GetMethod(nameof(MultipleMethods.VoidParameterlessMethod)).MetadataToken);

            Assert.Equal(
                "System.Void AsmResolver.DotNet.TestCases.Methods.MultipleMethods::VoidParameterlessMethod()",
                method.FullName);
        }

        [Fact]
        public void ReadSingleParameterMethodFullName()
        {
            var module = ModuleDefinition.FromFile(typeof(MultipleMethods).Assembly.Location);
            var method = (MethodDefinition) module.LookupMember(
                typeof(MultipleMethods).GetMethod(nameof(MultipleMethods.SingleParameterMethod)).MetadataToken);

            Assert.Equal(
                "System.Void AsmResolver.DotNet.TestCases.Methods.MultipleMethods::SingleParameterMethod(System.Int32)",
                method.FullName);
        }

        [Fact]
        public void ReadMultipleParametersMethodFullName()
        {
            var module = ModuleDefinition.FromFile(typeof(MultipleMethods).Assembly.Location);
            var method = (MethodDefinition) module.LookupMember(
                typeof(MultipleMethods).GetMethod(nameof(MultipleMethods.MultipleParameterMethod)).MetadataToken);

            Assert.Equal(
                "System.Void AsmResolver.DotNet.TestCases.Methods.MultipleMethods::MultipleParameterMethod(System.Int32, System.String, AsmResolver.DotNet.TestCases.Methods.MultipleMethods)",
                method.FullName);
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
                module.PEKind = OptionalHeaderMagic.PE32;
                module.MachineType = MachineType.I386;
            }
            else
            {
                module.FileCharacteristics |= Characteristics.Dll;
                module.PEKind = OptionalHeaderMagic.PE32Plus;
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

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ReadExportMethodByName(bool is32Bit)
        {
            var module = ModuleDefinition.FromBytes(is32Bit
                ? Properties.Resources.MyLibrary_X86
                : Properties.Resources.MyLibrary_X64);

            const int methodCount = 3;

            int matchedMethods = 0;
            foreach (var method in module.GetOrCreateModuleType().Methods)
            {
                if (method.Name!.Value.StartsWith("MyMethod"))
                {
                    Assert.True(method.ExportInfo.HasValue);
                    Assert.Equal(method.Name, method.ExportInfo.Value.Name);
                    matchedMethods++;
                }
            }

            Assert.Equal(methodCount, matchedMethods);
        }

        [SkippableTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void PersistExportMethodByName(bool is32Bit)
        {
            Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.Windows), NonWindowsPlatform);

            const int methodCount = 3;

            var assembly = CreateDummyLibraryWithExport(methodCount, is32Bit);
            var runner = _fixture.GetRunner<NativePERunner>();

            string suffix = is32Bit ? "x86" : "x64";

            // Write library to temp dir.
            string libraryPath = Path.ChangeExtension(runner.GetTestExecutablePath(
                nameof(MethodDefinitionTest),
                nameof(PersistExportMethodByName),
                $"MyLibrary.{suffix}.dll"), ".dll");
            assembly.Write(libraryPath);

            // Write caller to temp dir.
            string callerPath = runner.GetTestExecutablePath(
                nameof(MethodDefinitionTest),
                nameof(PersistExportMethodByName),
                $"CallManagedExport.{suffix}.exe");
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

        [Fact]
        public void SingleExportWithOrdinalShouldUpdateBaseOrdinal()
        {
            const uint ordinal = 100u;

            var module = CreateDummyLibraryWithExport(1, false).ManifestModule!;

            var method = module.GetModuleType()!.Methods.First(m => m.Name == "MyMethod0");
            method.ExportInfo = new UnmanagedExportInfo(ordinal, "MyMethod0", VTableType.VTable32Bit);

            var image = module.ToPEImage();
            Assert.NotNull(image.Exports);
            var entry = Assert.Single(image.Exports.Entries);
            Assert.Equal(ordinal, entry.Ordinal);
            Assert.Equal(ordinal, image.Exports.BaseOrdinal);
        }

        [Theory]
        [InlineData(new[] {0, 1, 2})]
        [InlineData(new[] {2, 1, 0})]
        [InlineData(new[] {1, 0, 2})]
        public void MultipleExportsWithFixedOrdinalInSequenceShouldUpdateBaseOrdinal(int[] order)
        {
            string[] names =
            {
                "MyMethod0",
                "MyMethod1",
                "MyMethod2",
            };
            const uint baseOrdinal = 100u;

            var module = CreateDummyLibraryWithExport(3, false).ManifestModule!;

            var methods = module.GetModuleType()!.Methods;
            var method1 = methods.First(m => m.Name == names[order[0]]);
            var method2 = methods.First(m => m.Name == names[order[1]]);
            var method3 = methods.First(m => m.Name == names[order[2]]);
            method1.ExportInfo = new UnmanagedExportInfo(baseOrdinal, names[order[0]], VTableType.VTable32Bit);
            method2.ExportInfo = new UnmanagedExportInfo(baseOrdinal + 1, names[order[1]], VTableType.VTable32Bit);
            method3.ExportInfo = new UnmanagedExportInfo(baseOrdinal + 2, names[order[2]], VTableType.VTable32Bit);

            var image = module.ToPEImage();
            Assert.NotNull(image.Exports);
            Assert.Equal(3, image.Exports.Entries.Count);
            Assert.Equal(baseOrdinal, image.Exports.Entries[0].Ordinal);
            Assert.Equal(names[order[0]], image.Exports.Entries[0].Name);
            Assert.Equal(baseOrdinal + 1, image.Exports.Entries[1].Ordinal);
            Assert.Equal(names[order[1]], image.Exports.Entries[1].Name);
            Assert.Equal(baseOrdinal + 2, image.Exports.Entries[2].Ordinal);
            Assert.Equal(names[order[2]], image.Exports.Entries[2].Name);
            Assert.Equal(baseOrdinal, image.Exports.BaseOrdinal);
        }

        [Fact]
        public void PreferInsertingFloatingExportsBeforeFixedExports()
        {
            const uint ordinal = 100u;

            var module = CreateDummyLibraryWithExport(2, false).ManifestModule!;

            var methods = module.GetModuleType()!.Methods;
            var method1 = methods.First(m => m.Name == "MyMethod0");
            var method2 = methods.First(m => m.Name == "MyMethod1");
            method1.ExportInfo = new UnmanagedExportInfo(ordinal, "MyMethod0", VTableType.VTable32Bit);
            method2.ExportInfo = new UnmanagedExportInfo("MyMethod1", VTableType.VTable32Bit);

            var image = module.ToPEImage();
            Assert.NotNull(image.Exports);
            Assert.Equal(2, image.Exports.Entries.Count);
            Assert.Equal("MyMethod1", image.Exports.Entries[0].Name);
            Assert.Equal("MyMethod0", image.Exports.Entries[1].Name);
            Assert.Equal(ordinal - 1, image.Exports.BaseOrdinal);
        }

        [Fact]
        public void AppendFloatingExportsToEndIfNoSpaceBeforeFixedExports()
        {
            const uint ordinal = 2u;

            var module = CreateDummyLibraryWithExport(4, false).ManifestModule!;

            var methods = module.GetModuleType()!.Methods;
            var method1 = methods.First(m => m.Name == "MyMethod0");
            var method2 = methods.First(m => m.Name == "MyMethod1");
            var method3 = methods.First(m => m.Name == "MyMethod2");
            var method4 = methods.First(m => m.Name == "MyMethod3");
            method1.ExportInfo = new UnmanagedExportInfo(ordinal, "MyMethod0", VTableType.VTable32Bit);
            method2.ExportInfo = new UnmanagedExportInfo("MyMethod1", VTableType.VTable32Bit);
            method3.ExportInfo = new UnmanagedExportInfo("MyMethod2", VTableType.VTable32Bit);
            method4.ExportInfo = new UnmanagedExportInfo("MyMethod3", VTableType.VTable32Bit);

            var image = module.ToPEImage();
            Assert.NotNull(image.Exports);
            Assert.Equal(4, image.Exports.Entries.Count);
            Assert.Equal("MyMethod0", image.Exports.Entries[1].Name);
            Assert.Equal(1u, image.Exports.BaseOrdinal);
        }

        [Fact]
        public void PreferInsertingFloatingExportsInGapsBetweenFixedExports()
        {
            const uint ordinal1 = 1u;
            const uint ordinal2 = 3u;

            var module = CreateDummyLibraryWithExport(3, false).ManifestModule!;

            var methods = module.GetModuleType()!.Methods;
            var method1 = methods.First(m => m.Name == "MyMethod0");
            var method2 = methods.First(m => m.Name == "MyMethod1");
            var method3 = methods.First(m => m.Name == "MyMethod2");
            method1.ExportInfo = new UnmanagedExportInfo(ordinal1, "MyMethod0", VTableType.VTable32Bit);
            method2.ExportInfo = new UnmanagedExportInfo(ordinal2, "MyMethod1", VTableType.VTable32Bit);
            method3.ExportInfo = new UnmanagedExportInfo( "MyMethod2", VTableType.VTable32Bit);

            var image = module.ToPEImage();
            Assert.NotNull(image.Exports);
            Assert.Equal("MyMethod0", image.Exports.Entries[0].Name);
            Assert.Equal("MyMethod2", image.Exports.Entries[1].Name);
            Assert.Equal("MyMethod1", image.Exports.Entries[2].Name);
            Assert.Equal(ordinal1, image.Exports.BaseOrdinal);
        }

        [Fact]
        public void FillUpExportGapsWithDummyExports()
        {
            const uint ordinal1 = 25u;
            const uint ordinal2 = 50u;
            const uint ordinal3 = 75u;

            var module = CreateDummyLibraryWithExport(3, false).ManifestModule!;

            var methods = module.GetModuleType()!.Methods;
            var method1 = methods.First(m => m.Name == "MyMethod0");
            var method2 = methods.First(m => m.Name == "MyMethod1");
            var method3 = methods.First(m => m.Name == "MyMethod2");
            method1.ExportInfo = new UnmanagedExportInfo(ordinal1, "MyMethod0", VTableType.VTable32Bit);
            method2.ExportInfo = new UnmanagedExportInfo(ordinal2, "MyMethod1", VTableType.VTable32Bit);
            method3.ExportInfo = new UnmanagedExportInfo(ordinal3, "MyMethod2", VTableType.VTable32Bit);

            var image = module.ToPEImage();
            Assert.NotNull(image.Exports);
            Assert.Equal(51, image.Exports.Entries.Count);

            foreach (var entry in image.Exports.Entries)
            {
                switch (entry.Ordinal)
                {
                    case ordinal1:
                        Assert.Equal("MyMethod0", entry.Name);
                        Assert.NotNull(entry.Address.GetSegment());
                        break;
                    case ordinal2:
                        Assert.Equal("MyMethod1", entry.Name);
                        Assert.NotNull(entry.Address.GetSegment());
                        break;
                    case ordinal3:
                        Assert.Equal("MyMethod2", entry.Name);
                        Assert.NotNull(entry.Address.GetSegment());
                        break;
                    default:
                        Assert.False(entry.IsByName);
                        Assert.Null(entry.Address.GetSegment());
                        break;
                }
            }

            Assert.Equal(ordinal1, image.Exports.BaseOrdinal);
        }

        [Theory]
        [InlineData("NonGenericMethodInNonGenericType",
            "System.Void AsmResolver.DotNet.TestCases.Generics.NonGenericType::NonGenericMethodInNonGenericType()")]
        [InlineData("GenericMethodInNonGenericType",
            "System.Void AsmResolver.DotNet.TestCases.Generics.NonGenericType::GenericMethodInNonGenericType<U1, U2, U3>()")]
        [InlineData("GenericMethodWithConstraints",
            "System.Void AsmResolver.DotNet.TestCases.Generics.NonGenericType::GenericMethodWithConstraints<T1, T2>()")]
        public void MethodFullNameTests(string methodName, string expectedFullName)
        {
            var module = ModuleDefinition.FromFile(typeof(NonGenericType).Assembly.Location);
            var method = module
                .TopLevelTypes.First(t => t.Name == nameof(NonGenericType))
                .Methods.First(m => m.Name == methodName);

            Assert.Equal(expectedFullName, method.FullName);
        }

        [Fact]
        public void CreateParameterlessConstructor()
        {
            var module = ModuleDefinition.FromFile(typeof(Constructors).Assembly.Location);
            var ctor = MethodDefinition.CreateConstructor(module);

            Assert.True(ctor.IsConstructor);
            Assert.Empty(ctor.Parameters);
            Assert.NotNull(ctor.CilMethodBody);
            Assert.Equal(CilOpCodes.Ret, Assert.Single(ctor.CilMethodBody.Instructions).OpCode);
        }

        [Fact]
        public void CreateConstructor()
        {
            var module = ModuleDefinition.FromFile(typeof(Constructors).Assembly.Location);
            var factory = module.CorLibTypeFactory;
            var ctor = MethodDefinition.CreateConstructor(module, factory.Int32, factory.Double);

            Assert.True(ctor.IsConstructor);
            Assert.Equal(new[] {factory.Int32, factory.Double}, ctor.Parameters.Select(x => x.ParameterType));
        }
    }
}
