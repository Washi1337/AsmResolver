using System.Collections.Generic;
using System.Linq;
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
            public string? Name { get; set; }
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

            AssertStructure(expected, peImage.Resources!);
        }

        [Fact]
        public void MaliciousSelfLoop()
        {
            var peImage = PEImage.FromBytes(Properties.Resources.HelloWorld_MaliciousWin32ResLoop,
                new PEReaderParameters(EmptyErrorListener.Instance));

            const int maxDirCount = 20;
            int dirCount = 0;

            var stack = new Stack<IResourceEntry>();
            stack.Push(peImage.Resources!);
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
            var peImage = PEImage.FromBytes(Properties.Resources.HelloWorld_MaliciousWin32ResDirName,
                new PEReaderParameters(EmptyErrorListener.Instance));
            Assert.Null(peImage.Resources!.Entries[0].Name);
        }

        [Fact]
        public void MaliciousDirectoryOffset()
        {
            var peImage = PEImage.FromBytes(Properties.Resources.HelloWorld_MaliciousWin32ResDirOffset,
                new PEReaderParameters(EmptyErrorListener.Instance));

            var entry = peImage.Resources!.Entries[0];
            Assert.Equal(16u, entry.Id);
            Assert.True(entry.IsDirectory);

            var directory = (IResourceDirectory) entry;
            Assert.Empty(directory.Entries);
        }

        [Fact]
        public void MaliciousDataOffset()
        {
            var peImage = PEImage.FromBytes(Properties.Resources.HelloWorld_MaliciousWin32ResDataOffset,
                new PEReaderParameters(EmptyErrorListener.Instance));

            var directory = (IResourceDirectory) peImage.Resources!.Entries[0];
            directory = (IResourceDirectory) directory.Entries[0];
            var data = (IResourceData) directory.Entries[0];

            Assert.Null(data.Contents);
        }

        [Fact]
        public void GetNonExistingDirectory()
        {
            var root = new ResourceDirectory(0u);
            Assert.Throws<KeyNotFoundException>(() => root.GetDirectory(1));
            Assert.False(root.TryGetDirectory(1, out _));
        }

        [Fact]
        public void GetNonExistingData()
        {
            var root = new ResourceDirectory(0u);
            Assert.Throws<KeyNotFoundException>(() => root.GetData(1));
            Assert.False(root.TryGetData(1, out _));
        }

        [Fact]
        public void GetExistingDirectory()
        {
            var root = new ResourceDirectory(0u);
            var stringDirectory = new ResourceDirectory(ResourceType.String);
            root.Entries.Add(stringDirectory);

            Assert.Same(stringDirectory, root.GetDirectory(ResourceType.String));
            Assert.Same(stringDirectory, root.GetDirectory((uint) ResourceType.String));
            Assert.True(root.TryGetDirectory(ResourceType.String, out var result));
            Assert.Same(stringDirectory, result);
            Assert.True(root.TryGetDirectory((uint) ResourceType.String, out result));
            Assert.Same(stringDirectory, result);

            Assert.Throws<KeyNotFoundException>(() => root.GetData((uint) ResourceType.String));
            Assert.False(root.TryGetData((uint) ResourceType.String, out _));
        }

        [Fact]
        public void GetExistingData()
        {
            var root = new ResourceDirectory(0u);
            var dataEntry = new ResourceData(1234u, new DataSegment(new byte[] { 1, 2, 3, 4 }));
            root.Entries.Add(dataEntry);

            Assert.Throws<KeyNotFoundException>(() => root.GetDirectory(1234u));
            Assert.False(root.TryGetDirectory(1234u, out _));

            Assert.Same(dataEntry, root.GetData(1234u));
            Assert.True(root.TryGetData(1234u, out var result));
            Assert.Same(dataEntry, result);
        }

        [Fact]
        public void AddNewDirectory()
        {
            var root = new ResourceDirectory(0u);

            Assert.Empty(root.Entries);

            var directory = new ResourceDirectory(ResourceType.String);
            root.InsertOrReplaceEntry(directory);

            Assert.Same(directory, Assert.Single(root.Entries));
        }

        [Fact]
        public void AddSecondDirectory()
        {
            var root = new ResourceDirectory(0u)
            {
                Entries = { new ResourceDirectory(1234u) }
            };

            Assert.Single(root.Entries);

            var directory = new ResourceDirectory(5678u);
            root.InsertOrReplaceEntry(directory);

            Assert.Equal(2, root.Entries.Count);
        }

        [Fact]
        public void ReplaceDirectoryWithDirectory()
        {
            var root = new ResourceDirectory(0u)
            {
                Entries = { new ResourceDirectory(1234u) }
            };

            var oldDirectory = root.GetDirectory(1234u);

            var newDirectory = new ResourceDirectory(1234u);
            root.InsertOrReplaceEntry(newDirectory);

            Assert.NotSame(oldDirectory, root.GetEntry(1234u));
            Assert.Same(newDirectory, Assert.Single(root.Entries));
        }

        [Fact]
        public void ReplaceDirectoryWithData()
        {
            var root = new ResourceDirectory(0u)
            {
                Entries = { new ResourceDirectory(1234u) }
            };

            var oldDirectory = root.GetDirectory(1234u);

            var newEntry = new ResourceData(1234u, new DataSegment(new byte[] { 1, 2, 3, 4 }));
            root.InsertOrReplaceEntry(newEntry);

            Assert.NotSame(oldDirectory, root.GetEntry(1234u));
            Assert.Same(newEntry, Assert.Single(root.Entries));
        }

        [Fact]
        public void RemoveNonExistingEntry()
        {
            var root = new ResourceDirectory(0u)
            {
                Entries = { new ResourceDirectory(1234u) }
            };

            Assert.False(root.RemoveEntry(5678u));
            Assert.Single(root.Entries);
        }

        [Fact]
        public void RemoveExistingEntry()
        {
            var root = new ResourceDirectory(0u)
            {
                Entries =
                {
                    new ResourceDirectory(1234u),
                    new ResourceDirectory(5678u)
                }
            };

            Assert.True(root.RemoveEntry(1234u));
            Assert.Single(root.Entries);
        }

        [Theory]
        [InlineData(ResourceType.Icon, new[] {ResourceType.Icon, ResourceType.String, ResourceType.Version})]
        [InlineData(ResourceType.RcData, new[] {ResourceType.String, ResourceType.RcData, ResourceType.Version})]
        [InlineData(ResourceType.Manifest, new[] {ResourceType.String, ResourceType.Version, ResourceType.Manifest})]
        [InlineData(ResourceType.String, new[] {ResourceType.String, ResourceType.Version})]
        public void InsertShouldPreserveOrder(ResourceType insertedEntry, ResourceType[] expected)
        {
            var image = new PEImage();

            image.Resources = new ResourceDirectory(0u);
            image.Resources.Entries.Add(new ResourceDirectory(ResourceType.String));
            image.Resources.Entries.Add(new ResourceDirectory(ResourceType.Version));
            image.Resources.InsertOrReplaceEntry(new ResourceDirectory(insertedEntry));

            Assert.Equal(expected, image.Resources.Entries.Select(x => (ResourceType) x.Id));
        }

    }
}
