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
        public void RebuildNetPENoChange()
        {
            var peFile = PEFile.FromBytes(Properties.Resources.HelloWorld);
            _fixture.RebuildAndRunExe(nameof(PEFileTest), nameof(RebuildNetPENoChange), 
                peFile, "HelloWorld", "Hello World!" + Environment.NewLine);
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
            _fixture.RebuildAndRunExe(nameof(PEFileTest), nameof(RebuildNetPEAddSection), 
                peFile, fileName, "Hello World!" + Environment.NewLine);

            // Read the new file.
            var newPEFile = PEFile.FromFile(_fixture.GetTestExecutable(
                nameof(PEFileTest), nameof(RebuildNetPEAddSection), fileName));

            // Verify the section and its data is present:
            var newSection = newPEFile.Sections.First(s => s.Header.Name == sectionName);
            var newData = new byte[sectionData.Length];
            
            Assert.Equal(sectionData.Length, newSection.Contents
                .CreateReader()
                .ReadBytes(newData, 0, newData.Length));
            Assert.Equal(sectionData, newData);
        }
        
    }
}