using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using AsmResolver.DotNet.Bundles;
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
            AssertReadWriteManifestWindowsPreservesOutput(
                Properties.Resources.HelloWorld_SingleFile_V1,
                "3.1",
                "HelloWorld.dll",
                $"Hello, World!{Environment.NewLine}");
        }

        [SkippableFact]
        public void WriteBundleManifestV2Windows()
        {
            Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.Windows));
            AssertReadWriteManifestWindowsPreservesOutput(
                Properties.Resources.HelloWorld_SingleFile_V2,
                "5.0",
                "HelloWorld.dll",
                $"Hello, World!{Environment.NewLine}");
        }

        [SkippableFact]
        public void WriteBundleManifestV6Windows()
        {
            Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.Windows));
            AssertReadWriteManifestWindowsPreservesOutput(
                Properties.Resources.HelloWorld_SingleFile_V6,
                "6.0",
                "HelloWorld.dll",
                $"Hello, World!{Environment.NewLine}");
        }

        private void AssertReadWriteManifestWindowsPreservesOutput(
            byte[] inputFile,
            string sdkVersion,
            string fileName,
            string expectedOutput)
        {
            var manifest = BundleManifest.FromBytes(inputFile);

            string sdkPath = Path.Combine(DotNetCorePathProvider.DefaultInstallationPath!, "sdk");
            string? sdkVersionPath = null;
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

            string appHostPathTemplate = Path.Combine(sdkVersionPath, "AppHostTemplate", "apphost.exe");

            using var stream = new MemoryStream();
            manifest.WriteUsingTemplate(appHostPathTemplate, stream, fileName);

            var newManifest = BundleManifest.FromBytes(stream.ToArray());
            AssertBundlesAreEqual(manifest, newManifest);

            string output = _fixture
                .GetRunner<PERunner>()
                .RunAndCaptureOutput(Path.ChangeExtension(fileName, ".exe"), stream.ToArray());
            Assert.Equal(expectedOutput, output);
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
