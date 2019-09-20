using System;
using AsmResolver.PE.File.Headers;
using AsmResolver.Tests;
using Xunit;

namespace AsmResolver.PE.File.Tests
{
    public class PEFileTest : IClassFixture<TemporaryDirectoryFixture>
    {
        private readonly TemporaryDirectoryFixture _fixture;

        public PEFileTest(TemporaryDirectoryFixture fixture)
        {
            _fixture = fixture;
        }
        
        [Fact]
        public void RebuildNetPENoChange()
        {
            var peFile = PEFile.FromBytes(Properties.Resources.HelloWorld);
            _fixture.RebuildAndRunExe(nameof(PEFileTest), nameof(RebuildNetPENoChange), 
                peFile, "HelloWorld", "Hello World!" + Environment.NewLine);
        }

    }
}