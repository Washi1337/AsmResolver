using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using AsmResolver.DotNet.Serialized;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.TestCases.NestedClasses;
using AsmResolver.IO;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.File;
using AsmResolver.PE.Win32Resources;
using Xunit;
using FileAttributes = AsmResolver.PE.DotNet.Metadata.Tables.FileAttributes;
using TypeAttributes = AsmResolver.PE.DotNet.Metadata.Tables.TypeAttributes;

namespace AsmResolver.DotNet.Tests
{
    public class ModuleDefinitionTest
    {
        private static readonly SignatureComparer Comparer = new();
        private const string NonWindowsPlatform = "Test loads a module from a base address, which is only supported on Windows.";

        private static ModuleDefinition Rebuild(ModuleDefinition module)
        {
            using var stream = new MemoryStream();
            module.Write(stream);
            return ModuleDefinition.FromBytes(stream.ToArray(), TestReaderParameters);
        }

        [SkippableFact]
        public void LoadFromModule()
        {
            Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.Windows), NonWindowsPlatform);

            var reflectionModule = typeof(ModuleDefinition).Assembly.ManifestModule;
            var module = ModuleDefinition.FromModule(reflectionModule, TestReaderParameters);
            Assert.Equal(reflectionModule.Name, module.Name);
        }

        [SkippableFact]
        public void LoadFromDynamicModule()
        {
            Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.Windows), NonWindowsPlatform);

            var reflectionModule = Assembly.Load(Properties.Resources.ActualLibrary).ManifestModule;
            var module = ModuleDefinition.FromModule(reflectionModule, TestReaderParameters);
            Assert.Equal("ActualLibrary.dll", module.Name);
        }

        [Fact]
        public void LoadFromFileShouldPopulateResolverSearchDirectories()
        {
            string path = typeof(ModuleDefinitionTest).Assembly.Location;
            var module = ModuleDefinition.FromFile(path, TestReaderParameters);

            Assert.Contains(
                Path.GetDirectoryName(path),
                ((AssemblyResolverBase) module.MetadataResolver.AssemblyResolver).SearchDirectories);
        }

        [Fact]
        public void LoadFromBytesShouldLeaveSearchDirectoriesEmpty()
        {
            string path = typeof(ModuleDefinitionTest).Assembly.Location;
            var module = ModuleDefinition.FromBytes(File.ReadAllBytes(path), TestReaderParameters);

            Assert.DoesNotContain(
                Path.GetDirectoryName(path),
                ((AssemblyResolverBase) module.MetadataResolver.AssemblyResolver).SearchDirectories);
        }

        [Fact]
        public void LoadFromBytesWithWorkingDirectoryShouldPopulateSearchDirectories()
        {
            string path = typeof(ModuleDefinitionTest).Assembly.Location;
            var module = ModuleDefinition.FromBytes(
                File.ReadAllBytes(path),
                new ModuleReaderParameters(Path.GetDirectoryName(path)));

            Assert.Contains(
                Path.GetDirectoryName(path),
                ((AssemblyResolverBase) module.MetadataResolver.AssemblyResolver).SearchDirectories);
        }

        [Fact]
        public void LoadFromFileWithSameWorkingDirectoryShouldNotPopulateSearchDirectoriesTwice()
        {
            string path = typeof(ModuleDefinitionTest).Assembly.Location;
            var module = ModuleDefinition.FromFile(path,
                new ModuleReaderParameters(Path.GetDirectoryName(path)));

            Assert.Equal(1, ((AssemblyResolverBase) module.MetadataResolver.AssemblyResolver)
                .SearchDirectories.Count(x => x == Path.GetDirectoryName(path)));
        }

        [Fact]
        public void LoadFromFileWithDifferentWorkingDirectoryShouldPopulateSearchDirectoriesTwice()
        {
            string path = typeof(ModuleDefinitionTest).Assembly.Location;
            string otherPath = @"C:\other\path";
            var module = ModuleDefinition.FromFile(path, new ModuleReaderParameters(otherPath));

            var searchDirectories = ((AssemblyResolverBase) module.MetadataResolver.AssemblyResolver).SearchDirectories;
            Assert.Contains(Path.GetDirectoryName(path), searchDirectories);
            Assert.Contains(otherPath, searchDirectories);
        }

        [Fact]
        public void ReadNameTest()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters);
            Assert.Equal("HelloWorld.exe", module.Name);
        }

        [Fact]
        public void ReadMvidFromNormalMetadata()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_DoubleGuidStream, TestReaderParameters);
            Assert.Equal(
                new Guid(new byte[]
                {
                    0x94, 0xe3, 0x75, 0xe2, 0x82, 0x8b, 0xac, 0x4c, 0xa3, 0x8c, 0xb3, 0x72, 0x4b, 0x81, 0xea, 0x05
                }), module.Mvid);
        }

        [Fact]
        public void ReadMvidFromEnCMetadata()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_DoubleGuidStream_EnC, TestReaderParameters);
            Assert.Equal(
                new Guid(new byte[]
                {
                    0x8F, 0x6C, 0x77, 0x06, 0xEE, 0x44, 0x65, 0x41, 0xB0, 0xF7, 0x2D, 0xBD, 0x12, 0x7F, 0xE2, 0x1B
                }), module.Mvid);
        }

        [Fact]
        public void NameIsPersistentAfterRebuild()
        {
            const string newName = "HelloMars.exe";

            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters);
            module.Name = newName;

            var newModule = Rebuild(module);
            Assert.Equal(newName, newModule.Name);
        }

        [Fact]
        public void ReadManifestModule()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters);
            Assert.NotNull(module.Assembly);
            Assert.Same(module, module.Assembly.ManifestModule);
        }

        [Fact]
        public void ReadTypesNoNested()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters);
            Assert.Equal(new Utf8String[] { "<Module>", "Program" }, module.TopLevelTypes.Select(t => t.Name));
        }

        [Fact]
        public void ReadTypesNested()
        {
            var module = ModuleDefinition.FromFile(typeof(TopLevelClass1).Assembly.Location, TestReaderParameters);
            Assert.Equal(new Utf8String[]
            {
                "<Module>",
                nameof(TopLevelClass1),
                nameof(TopLevelClass2)
            }, module.TopLevelTypes.Select(t => t.Name));
        }

        [Fact]
        public void ReadMaliciousNestedClassLoop()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_MaliciousNestedClassLoop, TestReaderParameters);
            Assert.Equal(new Utf8String[] { "<Module>", "Program" }, module.TopLevelTypes.Select(t => t.Name));
        }

        [Fact]
        public void ReadMaliciousNestedClassLoop2()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_MaliciousNestedClassLoop2, TestReaderParameters);
            Assert.Equal(
                new HashSet<Utf8String> { "<Module>", "Program", "MaliciousEnclosingClass" },
                new HashSet<Utf8String>(module.TopLevelTypes.Select(t => t.Name)));

            var enclosingClass = module.TopLevelTypes.First(x => x.Name == "MaliciousEnclosingClass");
            Assert.Single(enclosingClass.NestedTypes);
            Assert.Single(enclosingClass.NestedTypes[0].NestedTypes);
            Assert.Empty(enclosingClass.NestedTypes[0].NestedTypes[0].NestedTypes);
        }

        [Fact]
        public void HelloWorldReadAssemblyReferences()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters);
            Assert.Single(module.AssemblyReferences);
            Assert.Equal("mscorlib", module.AssemblyReferences[0].Name);
        }

        [Fact]
        public void LookupTypeReference()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters);
            var member = module.LookupMember(new MetadataToken(TableIndex.TypeRef, 12));

            var reference = Assert.IsAssignableFrom<TypeReference>(member);
            Assert.Equal("System", reference.Namespace);
            Assert.Equal("Object", reference.Name);
        }

        [Fact]
        public void LookupTypeReferenceStronglyTyped()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters);
            var reference = module.LookupMember<TypeReference>(new MetadataToken(TableIndex.TypeRef, 12));

            Assert.Equal("System", reference.Namespace);
            Assert.Equal("Object", reference.Name);
        }

        [Fact]
        public void TryLookupTypeReferenceStronglyTyped()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters);

            Assert.True(module.TryLookupMember(new MetadataToken(TableIndex.TypeRef, 12), out TypeReference reference));
            Assert.Equal("System", reference.Namespace);
            Assert.Equal("Object", reference.Name);
        }

        [Fact]
        public void LookupTypeDefinition()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters);
            var member = module.LookupMember(new MetadataToken(TableIndex.TypeDef, 2));

            var definition = Assert.IsAssignableFrom<TypeDefinition>(member);
            Assert.Equal("HelloWorld", definition.Namespace);
            Assert.Equal("Program", definition.Name);
        }

        [Fact]
        public void LookupTypeDefinitionStronglyTyped()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters);
            var definition = module.LookupMember<TypeDefinition>(new MetadataToken(TableIndex.TypeDef, 2));

            Assert.Equal("HelloWorld", definition.Namespace);
            Assert.Equal("Program", definition.Name);
        }

        [Fact]
        public void LookupAssemblyReference()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters);
            var member = module.LookupMember(new MetadataToken(TableIndex.AssemblyRef, 1));

            var reference = Assert.IsAssignableFrom<AssemblyReference>(member);
            Assert.Equal("mscorlib", reference.Name);
            Assert.Same(module.AssemblyReferences[0], reference);
        }

        [Fact]
        public void LookupAssemblyReferenceStronglyTyped()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters);
            var reference = module.LookupMember<AssemblyReference>(new MetadataToken(TableIndex.AssemblyRef, 1));

            Assert.Equal("mscorlib", reference.Name);
            Assert.Same(module.AssemblyReferences[0], reference);
        }

        [Fact]
        public void LookupModuleReference()
        {
            var module = ModuleDefinition.FromFile(Path.Combine("Resources", "Manifest.exe"), TestReaderParameters);
            var member = module.LookupMember(new MetadataToken(TableIndex.ModuleRef, 1));

            var reference =Assert.IsAssignableFrom<ModuleReference>(member);
            Assert.Equal("MyModel.netmodule", reference.Name);
            Assert.Same(module.ModuleReferences[0], reference);
        }

        [Fact]
        public void LookupModuleReferenceStronglyTyped()
        {
            var module = ModuleDefinition.FromFile(Path.Combine("Resources", "Manifest.exe"), TestReaderParameters);
            var reference = module.LookupMember<ModuleReference>(new MetadataToken(TableIndex.ModuleRef, 1));

            Assert.Equal("MyModel.netmodule", reference.Name);
            Assert.Same(module.ModuleReferences[0], reference);
        }

        [Fact]
        public void EmptyModuleShouldAlwaysContainCorLibReference()
        {
            // Issue #39 (https://github.com/Washi1337/AsmResolver/issues/39)

            var module = new ModuleDefinition("TestModule");
            var corLib = module.CorLibTypeFactory.CorLibScope;

            using var stream = new MemoryStream();
            module.Write(stream);

            var newModule = ModuleDefinition.FromBytes(stream.ToArray(), TestReaderParameters);
            var comparer = new SignatureComparer();
            Assert.Contains(newModule.AssemblyReferences, reference => comparer.Equals(corLib, reference));
        }

        [Fact]
        public void CorLibModuleHasModuleDefinitionCorLibScope()
        {
            // https://github.com/Washi1337/AsmResolver/issues/620

            var assembly = new AssemblyDefinition("mscorlib", new Version(4, 0, 0, 0));
            var module = new ModuleDefinition("mscorlib", null);
            assembly.Modules.Add(module);

            Assert.Same(module.CorLibTypeFactory.CorLibScope, module);
            Assert.Empty(module.AssemblyReferences);
        }

        [Fact]
        public void CreateNewCorLibFactory()
        {
            var module = new ModuleDefinition("MySampleModule");
            Assert.NotNull(module.CorLibTypeFactory);
            Assert.NotNull(module.CorLibTypeFactory.CorLibScope);
            Assert.NotNull(module.CorLibTypeFactory.Void);
        }

        [Fact]
        public void AddingTypeIsPersistentAfterRebuild()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters);

            var newType = new TypeDefinition("SomeNamespace", "SomeType",
                TypeAttributes.Class | TypeAttributes.Abstract | TypeAttributes.Sealed);
            module.TopLevelTypes.Add(newType);

            var newModule = Rebuild(module);
            var comparer = new SignatureComparer();
            Assert.Contains(newModule.TopLevelTypes, t => comparer.Equals(newType, t));
        }

        [Fact]
        public void NewFW40ModuleShouldAlwaysContainModuleType()
        {
            var module = new ModuleDefinition("TestModule");
            Assert.NotNull(module.GetModuleType());
        }

        [Fact]
        public void NewModuleShouldAlwaysContainModuleType()
        {
            var module = new ModuleDefinition("TestModule", KnownCorLibs.NetStandard_v2_1_0_0);
            Assert.NotNull(module.GetModuleType());
        }

        [Fact]
        public void GetOrCreateModuleConstructorShouldAddNewConstructorIfNotPresent()
        {
            var module = new ModuleDefinition("TestModule", KnownCorLibs.NetStandard_v2_1_0_0);
            var cctor = module.GetOrCreateModuleConstructor();
            Assert.Contains(cctor, module.GetModuleType().Methods);
        }

        [Fact]
        public void GetOrCreateModuleConstructorShouldGetExistingConstructorIfPresent()
        {
            var module = new ModuleDefinition("TestModule", KnownCorLibs.NetStandard_v2_1_0_0);
            var cctor = module.GetOrCreateModuleConstructor();
            var cctor2 = module.GetOrCreateModuleConstructor();
            Assert.Same(cctor, cctor2);
        }

        [Fact]
        public void PersistentResources()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters);

            // Add new directory.
            const string directoryName = "Test";
            var entryData = new byte[] { 0, 1, 2, 3, 4 };
            var directory = new ResourceDirectory(directoryName)
            {
                Entries =
                {
                    new ResourceDirectory(1)
                    {
                        Entries = { new ResourceData(1234, new DataSegment(entryData)) }
                    }
                }
            };
            module.NativeResourceDirectory!.Entries.Add(directory);

            // Write and rebuild.
            using var stream = new MemoryStream();
            module.Write(stream);
            var newModule = ModuleDefinition.FromBytes(stream.ToArray(), TestReaderParameters);

            // Assert contents.
            var newDirectory = (ResourceDirectory) newModule.NativeResourceDirectory!.Entries
                .First(entry => entry.Name == directoryName);
            newDirectory = (ResourceDirectory)newDirectory.Entries[0];

            var newData = (ResourceData)newDirectory.Entries[0];
            var newContents = (IReadableSegment)newData.Contents!;
            Assert.Equal(entryData, newContents.ToArray());
        }

        [Fact]
        public void PersistentTimeStamp()
        {
            var time = new DateTime(2020, 1, 2, 18, 30, 34);

            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters);
            module.TimeDateStamp = time;

            var image = module.ToPEImage();
            Assert.Equal(time, image.TimeDateStamp);
        }

        [Fact]
        public void PersistentExportedType()
        {
            var module = new ModuleDefinition("SomeModule.exe");

            var assembly = new AssemblyReference("SomeAssembly", new Version(1, 0, 0, 0));
            var type = new ExportedType(assembly, "SomeNamespace", "SomeType");

            module.AssemblyReferences.Add(assembly);
            module.ExportedTypes.Add(type);

            var newModule = Rebuild(module);
            var newType = Assert.Single(newModule.ExportedTypes);
            Assert.Equal(type, newType, new SignatureComparer());
        }

        [Fact]
        public void PersistentFileReferences()
        {
            var module = new ModuleDefinition("SomeModule.exe");

            var file = new FileReference("SubModule.netmodule", FileAttributes.ContainsMetadata);

            module.FileReferences.Add(file);

            var newModule = Rebuild(module);
            var newFile = Assert.Single(newModule.FileReferences);
            Assert.NotNull(newFile);
            Assert.Equal(file.Name, newFile.Name);
        }

        [Fact]
        public void DetectTargetNetFramework40()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters);
            Assert.True(module.OriginalTargetRuntime.IsNetFramework);
            Assert.Contains(DotNetRuntimeInfo.NetFramework, module.OriginalTargetRuntime.Name);
            Assert.Equal(4, module.OriginalTargetRuntime.Version.Major);
            Assert.Equal(0, module.OriginalTargetRuntime.Version.Minor);
        }

        [Fact]
        public void DetectTargetNetCore()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_NetCore, TestReaderParameters);
            Assert.True(module.OriginalTargetRuntime.IsNetCoreApp);
            Assert.Contains(DotNetRuntimeInfo.NetCoreApp, module.OriginalTargetRuntime.Name);
            Assert.Equal(2, module.OriginalTargetRuntime.Version.Major);
            Assert.Equal(2, module.OriginalTargetRuntime.Version.Minor);
        }

        [Fact]
        public void DetectTargetStandard()
        {
            var module = ModuleDefinition.FromFile(typeof(TestCases.Types.Class).Assembly.Location, TestReaderParameters);
            Assert.True(module.OriginalTargetRuntime.IsNetStandard);
            Assert.Contains(DotNetRuntimeInfo.NetStandard, module.OriginalTargetRuntime.Name);
            Assert.Equal(2, module.OriginalTargetRuntime.Version.Major);
        }

        [Fact]
        public void NewModuleShouldContainSingleReferenceToCorLib()
        {
            var module = new ModuleDefinition("SomeModule", KnownCorLibs.NetStandard_v2_0_0_0);
            var reference = Assert.Single(module.AssemblyReferences);
            Assert.Equal(KnownCorLibs.NetStandard_v2_0_0_0, reference, Comparer);
        }

        [Fact]
        public void RewriteSystemPrivateCoreLib()
        {
            string runtimePath = DotNetCorePathProvider.Default
                .GetRuntimePathCandidates("Microsoft.NETCore.App", new Version(3, 1, 0))
                .FirstOrDefault() ?? throw new InvalidOperationException(".NET Core 3.1 is not installed.");
            var module = ModuleDefinition.FromFile(Path.Combine(runtimePath, "System.Private.CoreLib.dll"), TestReaderParameters);

            using var stream = new MemoryStream();
            module.Write(stream);
        }

        [Fact]
        public void RewriteSystemRuntime()
        {
            string runtimePath = DotNetCorePathProvider.Default
                .GetRuntimePathCandidates("Microsoft.NETCore.App", new Version(3, 1, 0))
                .FirstOrDefault() ?? throw new InvalidOperationException(".NET Core 3.1 is not installed.");
            var module = ModuleDefinition.FromFile(Path.Combine(runtimePath, "System.Runtime.dll"), TestReaderParameters);

            using var stream = new MemoryStream();
            module.Write(stream);
        }

        [Fact]
        public void RewriteSystemPrivateXml()
        {
            string runtimePath = DotNetCorePathProvider.Default
                .GetRuntimePathCandidates("Microsoft.NETCore.App", new Version(3, 1, 0))
                .FirstOrDefault() ?? throw new InvalidOperationException(".NET Core 3.1 is not installed.");
            var module = ModuleDefinition.FromFile(Path.Combine(runtimePath, "System.Private.Xml.dll"), TestReaderParameters);

            using var stream = new MemoryStream();
            module.Write(stream);
        }

        [Fact]
        public void GetModuleTypeNetFramework()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.ModuleCctorNetFramework, TestReaderParameters);

            // Module type should exist.
            var type = module.GetModuleType();
            Assert.NotNull(type);
            Assert.Equal("CustomModuleType", type.Name);

            // Creating module type should give us the existing type.
            Assert.Same(type, module.GetOrCreateModuleType());
        }

        [Fact]
        public void GetModuleTypeNet6()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.ModuleCctorNet6, TestReaderParameters);

            // Module type should exist.
            var type = module.GetModuleType();
            Assert.NotNull(type);
            Assert.Equal("<Module>", type.Name);

            // Creating module type should give us the existing type.
            Assert.Same(type, module.GetOrCreateModuleType());
        }

        [Fact]
        public void GetModuleTypeAbsentNet6()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.ModuleCctorAbsentNet6, TestReaderParameters);

            // Module type should not exist.
            var type = module.GetModuleType();
            Assert.Null(type);

            // Creating should add it to the module.
            type = module.GetOrCreateModuleType();
            Assert.NotNull(type);
            Assert.Same(type, module.GetModuleType());
        }

        [Fact]
        public void GetModuleTypeLookalikeNet6()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.ModuleCctorLookalikeNet6, TestReaderParameters);

            // Module type should not exist.
            var type = module.GetModuleType();
            Assert.Null(type);

            // Creating should add it to the module.
            type = module.GetOrCreateModuleType();
            Assert.NotNull(type);
            Assert.Same(type, module.GetModuleType());
        }

        [Fact]
        public void CreateModuleTypeShouldHaveFirstRid()
        {
            var module = new ModuleDefinition("SomeModule");
            var type = module.GetOrCreateModuleType();
            Assert.Equal(1u, type.MetadataToken.Rid);
        }

        [Theory]
        [InlineData(false, false, false)]
        [InlineData(false, true, false)]
        [InlineData(true, false, true)]
        [InlineData(true, true, true)]
        public void IsLoadedAs32BitAnyCPUModule(bool assume32Bit, bool canLoadAs32Bit, bool expected)
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters);
            Assert.Equal(expected, module.IsLoadedAs32Bit(assume32Bit, canLoadAs32Bit));
        }

        [Theory]
        [InlineData(false, false, false)]
        [InlineData(false, true, true)]
        [InlineData(true, false, true)]
        [InlineData(true, true, true)]
        public void IsLoadedAs32BitAnyCPUModulePrefer32Bit(bool assume32Bit, bool canLoadAs32Bit, bool expected)
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters);
            module.IsBit32Preferred = true;
            Assert.Equal(expected, module.IsLoadedAs32Bit(assume32Bit, canLoadAs32Bit));
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void IsLoadedAs32Bit64BitModule(bool assume32Bit, bool canLoadAs32Bit)
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters);
            module.MachineType = MachineType.Amd64;
            Assert.False(module.IsLoadedAs32Bit(assume32Bit, canLoadAs32Bit));
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void IsLoadedAs32Bit32BitModule(bool assume32Bit, bool canLoadAs32Bit)
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters);
            module.MachineType = MachineType.I386;
            module.IsBit32Required = true;
            Assert.True(module.IsLoadedAs32Bit(assume32Bit, canLoadAs32Bit));
        }
    }
}
