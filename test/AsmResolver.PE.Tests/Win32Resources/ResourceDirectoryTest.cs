using System.Collections.Generic;
using AsmResolver.PE.Win32Resources;
using Xunit;

namespace AsmResolver.PE.Tests.Win32Resources
{
    public class ResourceDirectoryTest
    {
        private class ResourceEntryInfo
        {
            public uint Id { get; set; }
            public string Name { get; set; }
            public bool IsData { get; set; }
            public IList<ResourceEntryInfo> Entries { get; } = new List<ResourceEntryInfo>();
        }

        private void AssertStructure(ResourceEntryInfo expectedStructure, IResourceDirectoryEntry directory)
        {
            var expectedStack = new Stack<ResourceEntryInfo>();
            var stack = new Stack<IResourceDirectoryEntry>();
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
                    Assert.IsAssignableFrom<ResourceDataBase>(current);
                }
                else
                {
                    Assert.IsAssignableFrom<ResourceDirectoryBase>(current);
                    var subEntries = ((ResourceDirectoryBase) current).Entries;
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
            var peImage = PEImageBase.FromBytes(Properties.Resources.HelloWorld);
            
            var expected = new ResourceEntryInfo
            {
                Entries =
                {
                    new ResourceEntryInfo
                    {
                        Id = 16,
                        Entries =
                        {
                            new ResourceEntryInfo
                            {
                                Id = 1,
                                Entries =
                                {
                                    new ResourceEntryInfo
                                    {
                                        Id = 0,
                                        IsData = true
                                    }
                                }
                            }
                        }
                    },

                    new ResourceEntryInfo
                    {
                        Id = 24,
                        Entries =
                        {
                            new ResourceEntryInfo
                            {
                                Id = 1,
                                Entries =
                                {
                                    new ResourceEntryInfo
                                    {
                                        Id = 0,
                                        IsData = true
                                    }
                                }
                            }
                        }
                    }
                }
            };

            AssertStructure(expected, peImage.Resources);
        }
        
    }
}