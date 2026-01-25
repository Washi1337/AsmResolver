using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Builder;
using AsmResolver.IO;
using AsmResolver.PE;
using AsmResolver.PE.Builder;
using AsmResolver.PE.Exports;
using AsmResolver.PE.Exports.Builder;
using AsmResolver.PE.File;
using AsmResolver.Tests.Runners;
using Xunit;

namespace AsmResolver.DotNet.Tests
{
    public class ReproducibilityTest : IClassFixture<TemporaryDirectoryFixture>
    {
        [Fact]
        public void TextSection()
        {
            byte[] CreateImageAndHashTextSection()
            {
                const string UnsafeAssemblyName = "System.Runtime.CompilerServices.Unsafe";
                AssemblyDefinition assembly = new(UnsafeAssemblyName, new Version(4, 0, 0, 0));
                using var mscorlibStream = typeof(ReproducibilityTest).Assembly.GetManifestResourceStream("AsmResolver.DotNet.Tests.Resources.mscorlib_LC.dll");
                var mscorlib = AssemblyDefinition.FromStream(mscorlibStream);
                AssemblyReference corLibReference = new(mscorlib);
                ModuleDefinition module = new(UnsafeAssemblyName, corLibReference);
                module.AssemblyReferences.Add(corLibReference);
                assembly.Modules.Add(module);
                PEImage image = assembly.ManifestModule.ToPEImage(new ManagedPEImageBuilder(), throwOnNonFatalError: false);
                AsmResolver.PE.File.PEFile writingFile = new ManagedPEFileBuilder().CreateFile(image);
                using MemoryStream temp = new();
                BinaryStreamWriter writer = new(temp);
                PESection section = writingFile.Sections.First(section => section.Name == ".text");
                section.Contents?.Write(writer);
                return SHA1.HashData(temp.GetBuffer());
            }
            Assert.Equal(CreateImageAndHashTextSection(), CreateImageAndHashTextSection());
        }
    }
}
