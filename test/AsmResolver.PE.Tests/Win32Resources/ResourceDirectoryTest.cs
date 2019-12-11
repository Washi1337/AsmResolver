using System.Collections.Generic;
using AsmResolver.PE.Win32Resources;
using Xunit;

namespace AsmResolver.PE.Tests.Win32Resources
{
    public class ResourceDirectoryTest
    {
        private class ResourceEntryInfo
        {
            public ResourceEntryInfo(uint id)
            {
                Id = id;
            }

            public ResourceEntryInfo(string name)
            {
                Name = name;
            }
            
            public uint Id { get; set; }
            public string Name { get; set; }
            public bool IsData { get; set; }
            public IList<ResourceEntryInfo> Entries { get; } = new List<ResourceEntryInfo>();
        }

        private void AssertStructure(ResourceEntryInfo expectedStructure, IResourceEntry directory)
        {
            var expectedStack = new Stack<ResourceEntryInfo>();
            var stack = new Stack<IResourceEntry>();
            expectedStack.Push(expectedStructure);
            stack.Push(directory);

            while (stack.Count > 0)
            {
                var expected = expectedStack.Pop();
                var current = stack.Pop();

                Assert.Equal(expected.Name, current.Name);
                Assert.Equal(expected.Id, current.Id);

                if (expected.IsData)
                {
                    Assert.IsAssignableFrom<IResourceData>(current);
                }
                else
                {
                    Assert.IsAssignableFrom<IResourceDirectory>(current);
                    var subEntries = ((IResourceDirectory) current).Entries;
                    Assert.Equal(expected.Entries.Count, subEntries.Count);

                    for (int i = 0; i < subEntries.Count; i++)
                    {
                        expectedStack.Push(expected.Entries[i]);
                        stack.Push(subEntries[i]);
                    }
                }
            }
        }
        
        [Fact]
        public void DotNetHelloWorld()
        {
            var peImage = PEImage.FromBytes(Properties.Resources.HelloWorld);

            var expected = new ResourceEntryInfo(0)
            {
                Entries =
                {
                    new ResourceEntryInfo(16)
                    {
                        Entries =
                        {
                            new ResourceEntryInfo(1)
                            {
                                Entries =
                                {
                                    new ResourceEntryInfo(0) {IsData = true}
                                }
                            }
                        }
                    },

                    new ResourceEntryInfo(24)
                    {
                        Entries =
                        {
                            new ResourceEntryInfo(1)
                            {
                                Entries =
                                {
                                    new ResourceEntryInfo(0) { IsData = true }
                                }
                            }
                        }
                    }
                }
            };

            AssertStructure(expected, peImage.Resources);
        }

        [Fact]
        public void MaliciousSelfLoop()
        {
            var peImage = PEImage.FromBytes(Properties.Resources.HelloWorld_MaliciousWin32ResLoop);

            const int maxDirCount = 20;
            int dirCount = 0;
            
            var stack = new Stack<IResourceEntry>();
            stack.Push(peImage.Resources);
            while (stack.Count > 0)
            {
                var current = stack.Pop();
                if (current.IsDirectory)
                {
                    Assert.True(dirCount < maxDirCount, "Traversal reached limit of resource directories.");
                    
                    dirCount++;
                    foreach (var entry in ((IResourceDirectory) current).Entries)
                        stack.Push(entry);
                }
            }
        }

        [Fact]
        public void MaliciousDirectoryName()
        {
            var peImage = PEImage.FromBytes(Properties.Resources.HelloWorld_MaliciousWin32ResDirName);
            Assert.Null(peImage.Resources.Entries[0].Name);
        }

        [Fact]
        public void MaliciousDirectoryOffset()
        {
            var peImage = PEImage.FromBytes(Properties.Resources.HelloWorld_MaliciousWin32ResDirOffset);
            
            var entry = peImage.Resources.Entries[0];
            Assert.Equal(16u, entry.Id);
            Assert.True(entry.IsDirectory);

            var directory = (IResourceDirectory) entry;
            Assert.Empty(directory.Entries);
        }

        [Fact]
        public void MaliciousDataOffset()
        {
            var peImage = PEImage.FromBytes(Properties.Resources.HelloWorld_MaliciousWin32ResDataOffset);

            var directory = (IResourceDirectory) peImage.Resources.Entries[0];
            directory = (IResourceDirectory) directory.Entries[0];
            var data = (IResourceData) directory.Entries[0];
            
            Assert.Null(data.Contents);
        }
        
    }
}