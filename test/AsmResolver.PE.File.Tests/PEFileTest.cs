using System;
using System.Linq;
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
        public void ValidRvaToFileOffset()
        {
            var peFile = PEFile.FromBytes(Properties.Resources.HelloWorld);
            Assert.Equal(0x0000088Eu, peFile.RvaToFileOffset(0x0000268Eu));
        }

        [Fact]
        public void InvalidRvaToFileOffset()
        {
            var peFile = PEFile.FromBytes(Properties.Resources.HelloWorld);
            Assert.Throws<ArgumentOutOfRangeException>(() => peFile.RvaToFileOffset(0x3000));
        }

        [Fact]
        public void ValidFileOffsetToRva()
        {
            var peFile = PEFile.FromBytes(Properties.Resources.HelloWorld);
            Assert.Equal(0x0000268Eu, peFile.FileOffsetToRva(0x0000088Eu));
        }

        [Fact]
        public void InvalidFileOffsetToRva()
        {
            var peFile = PEFile.FromBytes(Properties.Resources.HelloWorld);
            Assert.Throws<ArgumentOutOfRangeException>(() => peFile.FileOffsetToRva(0x2000));
        }
        
        [Fact]
        public void RebuildNetPENoChange()
        {
            var peFile = PEFile.FromBytes(Properties.Resources.HelloWorld);
            _fixture.RebuildAndRunExe(peFile, "HelloWorld", "Hello World!" + Environment.NewLine);
        }

        [Fact]
        public void RebuildNetPEAddSection()
        {
            const string fileName = "HelloWorld";
            const string sectionName = ".test";
            var sectionData = new byte[] {1, 3, 3, 7};
            
            var peFile = PEFile.FromBytes(Properties.Resources.HelloWorld);
            
            // Add a new section.
            peFile.Sections.Add(new PESection(sectionName, SectionFlags.MemoryRead | SectionFlags.ContentInitializedData)
            {
                Contents = new DataSegment(sectionData)
            });

            // Rebuild and check if file is still runnable.
            _fixture.RebuildAndRunExe(peFile, fileName, "Hello World!" + Environment.NewLine);

            // Read the new file.
            var newPEFile = PEFile.FromFile(_fixture.GetTestExecutable(
                nameof(PEFileTest), nameof(RebuildNetPEAddSection), fileName));

            // Verify the section and its data is present:
            var newSection = newPEFile.Sections.First(s => s.Header.Name == sectionName);
            var newData = new byte[sectionData.Length];
            
            Assert.Equal(sectionData.Length, newSection
                .CreateReader()
                .ReadBytes(newData, 0, newData.Length));
            Assert.Equal(sectionData, newData);
        }
        
    }
}