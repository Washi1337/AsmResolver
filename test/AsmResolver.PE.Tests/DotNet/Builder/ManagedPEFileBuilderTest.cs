using System;
using System.Diagnostics;
using AsmResolver.PE.Code;
using AsmResolver.PE.DotNet;
using AsmResolver.PE.DotNet.Builder;
using AsmResolver.PE.DotNet.Metadata.Strings;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using AsmResolver.PE.File.Headers;
using AsmResolver.Tests;
using AsmResolver.Tests.Runners;
using Xunit;

namespace AsmResolver.PE.Tests.DotNet.Builder
{
    public class ManagedPEFileBuilderTest : IClassFixture<TemporaryDirectoryFixture>
    {
        private readonly TemporaryDirectoryFixture _fixture;

        public ManagedPEFileBuilderTest(TemporaryDirectoryFixture fixture)
        {
            _fixture = fixture;
        }
        
        [Fact]
        public void HelloWorldRebuild32BitNoChange()
        {
            // Read image
            var image = PEImage.FromBytes(Properties.Resources.HelloWorld);
            
            // Rebuild
            var builder = new ManagedPEFileBuilder();
            var peFile = builder.CreateFile(image);
            
            // Verify
            _fixture
                .GetRunner<FrameworkPERunner>()
                .RebuildAndRun(peFile, "HelloWorld", "Hello World!" + Environment.NewLine);
        }
        
        [Fact]
        public void HelloWorldRebuild64BitNoChange()
        {
            // Read image
            var image = PEImage.FromBytes(Properties.Resources.HelloWorld_X64);
            
            // Rebuild
            var builder = new ManagedPEFileBuilder();
            var peFile = builder.CreateFile(image);
            
            // Verify
            _fixture
                .GetRunner<FrameworkPERunner>()
                .RebuildAndRun(peFile, "HelloWorld", "Hello World!" + Environment.NewLine);
        }

        [Fact]
        public void HelloWorld32BitTo64Bit()
        {
            // Read image
            var image = PEImage.FromBytes(Properties.Resources.HelloWorld);
            
            // Change machine type and pe kind to 64-bit
            image.MachineType = MachineType.Amd64;
            image.PEKind = OptionalHeaderMagic.PE32Plus;
            
            // Rebuild
            var builder = new ManagedPEFileBuilder();
            var peFile = builder.CreateFile(image);
            
            // Verify
            _fixture
                .GetRunner<FrameworkPERunner>()
                .RebuildAndRun(peFile, "HelloWorld", "Hello World!" + Environment.NewLine);
        }

        [Fact]
        public void HelloWorld64BitTo32Bit()
        {
            // Read image
            var image = PEImage.FromBytes(Properties.Resources.HelloWorld_X64);
            
            // Change machine type and pe kind to 32-bit
            image.MachineType = MachineType.I386;
            image.PEKind = OptionalHeaderMagic.PE32;
            
            // Rebuild
            var builder = new ManagedPEFileBuilder();
            var peFile = builder.CreateFile(image);
            
            // Verify
            _fixture
                .GetRunner<FrameworkPERunner>()
                .RebuildAndRun(peFile, "HelloWorld", "Hello World!" + Environment.NewLine);
        }
        
    }
}