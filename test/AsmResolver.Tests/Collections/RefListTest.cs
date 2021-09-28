using AsmResolver.Collections;
using Xunit;

namespace AsmResolver.Tests.Collections
{
    public class RefListTest
    {
        [Fact]
        public void EmptyList()
        {
            Assert.Empty(new RefList<int>());
        }

        [Fact]
        public void AddItemShouldUpdateVersion()
        {
            var list = new RefList<int>();
            int version = list.Version;
            list.Add(1);
            Assert.Equal(1, Assert.Single(list));
            Assert.NotEqual(version, list.Version);
        }

        [Fact]
        public void InsertItemShouldUpdateVersion()
        {
            var list = new RefList<int> { 1, 2, 3 };
            int version = list.Version;
            list.Insert(1, 4);
            Assert.Equal(new[] { 1, 4, 2, 3 }, list);
            Assert.NotEqual(version, list.Version);
        }

        [Fact]
        public void RemoveItemShouldUpdateVersion()
        {
            var list = new RefList<int> { 1, 2, 3 };
            int version = list.Version;
            list.RemoveAt(1);
            Assert.Equal(new[] { 1, 3 }, list);
            Assert.NotEqual(version, list.Version);
        }

        [Fact]
        public void UpdateElementFromRefShouldUpdateList()
        {
            var list = new RefList<int> { 1, 2, 3 };
            ref int element = ref list.GetElementRef(1);
            element = 1337;
            Assert.Equal(new[] { 1, 1337, 3 }, list);
        }

    }
}
