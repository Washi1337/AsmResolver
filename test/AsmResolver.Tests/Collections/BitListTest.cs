using System.Linq;
using AsmResolver.Collections;
using Xunit;

namespace AsmResolver.Tests.Collections
{
    public class BitListTest
    {
        [Fact]
        public void Add()
        {
            var list = new BitList
            {
                true,
                false,
                true,
                true,
                false,
            };

            Assert.Equal(new[]
            {
                true,
                false,
                true,
                true,
                false
            }, list.ToArray());
        }

        [Fact]
        public void Insert()
        {
            var list = new BitList
            {
                true,
                false,
                true,
                true,
                false,
            };

            list.Insert(1, true);

            Assert.Equal(new[]
            {
                true,
                true,
                false,
                true,
                true,
                false
            }, list.ToArray());
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void InsertIntoLarge(bool parity)
        {
            var list = new BitList();
            for (int i = 0; i < 100; i++)
                list.Add(i % 2 == 0 == parity);

            list.Insert(0, !parity);

            Assert.Equal(101, list.Count);
            bool[] expected = Enumerable.Range(0, 101).Select(i => i % 2 == 1 == parity).ToArray();
            Assert.Equal(expected, list.ToArray());
        }

        [Fact]
        public void RemoveAt()
        {
            var list = new BitList
            {
                true,
                false,
                true,
                true,
                false,
            };

            list.RemoveAt(3);

            Assert.Equal(new[]
            {
                true,
                false,
                true,
                false,
            }, list.ToArray());
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void RemoveAtLarge(bool parity)
        {
            var list = new BitList();
            for (int i = 0; i < 100; i++)
                list.Add(i % 2 == 0 == parity);

            list.RemoveAt(0);

            Assert.Equal(99, list.Count);
            bool[] expected = Enumerable.Range(0, 99).Select(i => i % 2 == 1 == parity).ToArray();
            Assert.Equal(expected, list.ToArray());
        }
    }
}
