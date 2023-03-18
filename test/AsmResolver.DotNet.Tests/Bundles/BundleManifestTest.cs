using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using AsmResolver.DotNet.Bundles;
using AsmResolver.IO;
using AsmResolver.PE;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.File;
using AsmResolver.PE.File.Headers;
using AsmResolver.PE.Win32Resources.Version;
using AsmResolver.Tests.Runners;
using Xunit;

namespace AsmResolver.DotNet.Tests.Bundles
{
    public class BundleManifestTest : IClassFixture<TemporaryDirectoryFixture>
    {
        private readonly TemporaryDirectoryFixture _fixture;

        public BundleManifestTest(TemporaryDirectoryFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void ReadBundleManifestHeaderV1()
        {
            var manifest = BundleManifest.FromBytes(Properties.Resources.HelloWorld_SingleFile_V1);
            Assert.Equal(1u, manifest.MajorVersion);
            Assert.Equal("j7LK4is5ipe1CCtiafaTb8uhSOR7JhI=", manifest.BundleID);
            Assert.Equal(new[]
            {
                "HelloWorld.dll", "HelloWorld.deps.json", "HelloWorld.runtimeconfig.json"
            }, manifest.Files.Select(f => f.RelativePath));
        }

        [Fact]
        public void ReadBundleManifestHeaderV2()
        {
            var manifest = BundleManifest.FromBytes(Properties.Resources.HelloWorld_SingleFile_V2);
            Assert.Equal(2u, manifest.MajorVersion);
            Assert.Equal("poUQ+RBCefcEL4xrSAXdE2I5M+5D_Pk=", manifest.BundleID);
            Assert.Equal(new[]
            {
                "HelloWorld.dll", "HelloWorld.deps.json", "HelloWorld.runtimeconfig.json"
            }, manifest.Files.Select(f => f.RelativePath));
        }

        [Fact]
        public void ReadBundleManifestHeaderV6()
        {
            var manifest = BundleManifest.FromBytes(Properties.Resources.HelloWorld_SingleFile_V6);
            Assert.Equal(6u, manifest.MajorVersion);
            Assert.Equal("lc43r48XAQNxN7Cx8QQvO9JgZI5lqPA=", manifest.BundleID);
            Assert.Equal(new[]
            {
                "HelloWorld.dll", "HelloWorld.deps.json", "HelloWorld.runtimeconfig.json"
            }, manifest.Files.Select(f => f.RelativePath));
        }

        [SkippableFact]
        public void WriteBundleManifestV1Windows()
        {
            Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.Windows));
            AssertWriteManifestWindowsPreservesOutput(
                 BundleManifest.FromBytes(Properties.Resources.HelloWorld_SingleFile_V1),
                "3.1",
                "HelloWorld.dll",
                $"Hello, World!{Environment.NewLine}");
        }

        [SkippableFact]
        public void WriteBundleManifestV2Windows()
        {
            Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.Windows));
            AssertWriteManifestWindowsPreservesOutput(
                BundleManifest.FromBytes(Properties.Resources.HelloWorld_SingleFile_V2),
                "5.0",
                "HelloWorld.dll",
                $"Hello, World!{Environment.NewLine}");
        }

        [SkippableFact]
        public void WriteBundleManifestV6Windows()
        {
            Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.Windows));
            AssertWriteManifestWindowsPreservesOutput(
                BundleManifest.FromBytes(Properties.Resources.HelloWorld_SingleFile_V6),
                "6.0",
                "HelloWorld.dll",
                $"Hello, World!{Environment.NewLine}");
        }

        [SkippableFact]
        public void MarkFilesAsCompressed()
        {
            Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.Windows));
            var manifest = BundleManifest.FromBytes(Properties.Resources.HelloWorld_SingleFile_V6);
            manifest.Files.First(f => f.RelativePath == "HelloWorld.dll").Compress();

            using var stream = new MemoryStream();
            ulong address = manifest.WriteManifest(new BinaryStreamWriter(stream), false);

            var reader = new BinaryStreamReader(stream.ToArray());
            reader.Offset = address;
            var newManifest = BundleManifest.FromReader(reader);
            AssertBundlesAreEqual(manifest, newManifest);
        }

        [SkippableTheory()]
        [InlineData(SubSystem.WindowsCui)]
        [InlineData(SubSystem.WindowsGui)]
        public void WriteWithSubSystem(SubSystem subSystem)
        {
            Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.Windows));

            var manifest = BundleManifest.FromBytes(Properties.Resources.HelloWorld_SingleFile_V6);
            string appHostTemplatePath = FindAppHostTemplate("6.0");

            using var stream = new MemoryStream();
            var parameters = BundlerParameters.FromTemplate(appHostTemplatePath, "HelloWorld.dll");
            parameters.SubSystem = subSystem;
            manifest.WriteUsingTemplate(stream, parameters);

            var newFile = PEFile.FromBytes(stream.ToArray());
            Assert.Equal(subSystem, newFile.OptionalHeader.SubSystem);
        }

        [SkippableFact]
        public void WriteWithWin32Resources()
        {
            Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.Windows));

            var manifest = BundleManifest.FromBytes(Properties.Resources.HelloWorld_SingleFile_V6_WithResources);
            string appHostTemplatePath = FindAppHostTemplate("6.0");

            // Obtain expected version info.
            var oldImage = PEImage.FromBytes(Properties.Resources.HelloWorld_SingleFile_V6_WithResources);
            var versionInfo = VersionInfoResource.FromDirectory(oldImage.Resources!)!;

            // Bundle with PE image as template for PE headers and resources.
            using var stream = new MemoryStream();
            manifest.WriteUsingTemplate(stream, BundlerParameters.FromTemplate(
                File.ReadAllBytes(appHostTemplatePath),
                "HelloWorld.dll",
                oldImage));

            // Verify new file still runs as expected.
            string output = _fixture
                .GetRunner<NativePERunner>()
                .RunAndCaptureOutput("HelloWorld.exe", stream.ToArray());

            Assert.Equal($"Hello, World!{Environment.NewLine}", output);

            // Verify that resources were added properly.
            var newImage = PEImage.FromBytes(stream.ToArray());
            Assert.NotNull(newImage.Resources);
            var newVersionInfo = VersionInfoResource.FromDirectory(newImage.Resources);
            Assert.NotNull(newVersionInfo);
            Assert.Equal(versionInfo.FixedVersionInfo.FileVersion, newVersionInfo.FixedVersionInfo.FileVersion);
        }

        [Fact]
        public void NewManifestShouldGenerateBundleIdIfUnset()
        {
            var manifest = new BundleManifest(6);

            manifest.Files.Add(new BundleFile("HelloWorld.dll", BundleFileType.Assembly,
                Properties.Resources.HelloWorld_NetCore));
            manifest.Files.Add(new BundleFile("HelloWorld.runtimeconfig.json", BundleFileType.RuntimeConfigJson,
                Encoding.UTF8.GetBytes(@"{
    ""runtimeOptions"": {
        ""tfm"": ""net6.0"",
        ""includedFrameworks"": [
            {
                ""name"": ""Microsoft.NETCore.App"",
                ""version"": ""6.0.0""
            }
        ]
    }
}")));

            Assert.Null(manifest.BundleID);

            using var stream = new MemoryStream();
            manifest.WriteUsingTemplate(stream, BundlerParameters.FromTemplate(
                FindAppHostTemplate("6.0"),
                "HelloWorld.dll"));

            Assert.NotNull(manifest.BundleID);
        }

        [Fact]
        public void SameManifestContentsShouldResultInSameBundleID()
        {
            var manifest = BundleManifest.FromBytes(Properties.Resources.HelloWorld_SingleFile_V6);

            var newManifest = new BundleManifest(manifest.MajorVersion);
            foreach (var file in manifest.Files)
                newManifest.Files.Add(new BundleFile(file.RelativePath, file.Type, file.GetData()));

            Assert.Equal(manifest.BundleID, newManifest.GenerateDeterministicBundleID());
        }

        [Fact]
        public void PatchAndRepackageExistingBundleV6()
        {
            // Read manifest and locate main entry point file.
            var manifest = BundleManifest.FromBytes(Properties.Resources.HelloWorld_SingleFile_V6);
            var mainFile = manifest.Files.First(f => f.RelativePath.Contains("HelloWorld.dll"));

            // Patch entry point file.
            var module = ModuleDefinition.FromBytes(mainFile.GetData());
            module.ManagedEntryPointMethod!.CilMethodBody!
                .Instructions.First(i => i.OpCode.Code == CilCode.Ldstr)
                .Operand = "Hello, Mars!";

            using var moduleStream = new MemoryStream();
            module.Write(moduleStream);

            mainFile.Contents = new DataSegment(moduleStream.ToArray());
            mainFile.IsCompressed = false;

            // Repackage bundle using existing bundle as template.
            using var bundleStream = new MemoryStream();
            manifest.WriteUsingTemplate(bundleStream, BundlerParameters.FromExistingBundle(
                Properties.Resources.HelloWorld_SingleFile_V6,
                mainFile.RelativePath));

            // Verify application runs as expected.
            string output = _fixture
                .GetRunner<NativePERunner>()
                .RunAndCaptureOutput(
                    "HelloWorld.exe",
                    bundleStream.ToArray(),
                    null,
                    5000);

            Assert.Equal($"Hello, Mars!{Environment.NewLine}", output);
        }

        private void AssertWriteManifestWindowsPreservesOutput(
            BundleManifest manifest,
            string sdkVersion,
            string fileName,
            string expectedOutput,
            [CallerFilePath] string className = "File",
            [CallerMemberName] string methodName = "Method")
        {
            string appHostTemplatePath = FindAppHostTemplate(sdkVersion);

            using var stream = new MemoryStream();
            manifest.WriteUsingTemplate(stream, BundlerParameters.FromTemplate(appHostTemplatePath, fileName));

            var newManifest = BundleManifest.FromBytes(stream.ToArray());
            AssertBundlesAreEqual(manifest, newManifest);

            string output = _fixture
                .GetRunner<NativePERunner>()
                .RunAndCaptureOutput(
                    Path.ChangeExtension(fileName, ".exe"),
                    stream.ToArray(),
                    null,
                    5000,
                    className,
                    methodName);

            Assert.Equal(expectedOutput, output);
        }

        private static string FindAppHostTemplate(string sdkVersion)
        {
            string sdkPath = Path.Combine(DotNetCorePathProvider.DefaultInstallationPath!, "sdk");
            string sdkVersionPath = null;
            foreach (string dir in Directory.GetDirectories(sdkPath))
            {
                if (Path.GetFileName(dir).StartsWith(sdkVersion))
                {
                    sdkVersionPath = Path.Combine(dir);
                    break;
                }
            }

            if (string.IsNullOrEmpty(sdkVersionPath))
            {
                throw new InvalidOperationException(
                    $"Could not find the apphost template for .NET SDK version {sdkVersion}. This is an indication that the test environment does not have this SDK installed.");
            }

            string fileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "apphost.exe"
                : "apphost";

            string finalPath = Path.Combine(sdkVersionPath, "AppHostTemplate", fileName);
            if (!File.Exists(finalPath))
            {
                throw new InvalidOperationException(
                    $"Could not find the apphost template for .NET SDK version {sdkVersion}. This is an indication that the test environment does not have this SDK installed.");
            }

            return finalPath;
        }

        private static void AssertBundlesAreEqual(BundleManifest manifest, BundleManifest newManifest)
        {
            Assert.Equal(manifest.MajorVersion, newManifest.MajorVersion);
            Assert.Equal(manifest.MinorVersion, newManifest.MinorVersion);
            Assert.Equal(manifest.BundleID, newManifest.BundleID);

            Assert.Equal(manifest.Files.Count, newManifest.Files.Count);
            for (int i = 0; i < manifest.Files.Count; i++)
            {
                var file = manifest.Files[i];
                var newFile = newManifest.Files[i];
                Assert.Equal(file.Type, newFile.Type);
                Assert.Equal(file.RelativePath, newFile.RelativePath);
                Assert.Equal(file.IsCompressed, newFile.IsCompressed);
                Assert.Equal(file.GetData(), newFile.GetData());
            }
        }
    }
}
