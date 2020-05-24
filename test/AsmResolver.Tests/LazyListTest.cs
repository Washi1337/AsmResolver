using System.Collections.Generic;
using AsmResolver.Lazy;
using Xunit;

namespace AsmResolver.Tests
{
    public class LazyListTest
    {
        private class DummyLazyList : LazyList<int>
        {
            private readonly IList<int> _items;

            public bool InitializeMethodCalled
            {
                get;
                private set;
            }
            
            public DummyLazyList(IList<int> items)
            {
                _items = items;
            }
            
            protected override void Initialize()
            {
                foreach (var item in _items)
                    Items.Add(item);
                InitializeMethodCalled = true;
            }
        }

        [Fact]
        public void EnumeratorShouldEnumerateAllItems()
        {
            var actualItems = new[]
            {
                1,
                2,
                3
            };
            var list = new DummyLazyList(actualItems);

            Assert.Equal(actualItems, list);
        }

        [Fact]
        public void EnumeratorWithNoMoveNextShouldNotInitializeList()
        {
            var actualItems = new[]
            {
                1,
                2,
                3
            };
            var list = new DummyLazyList(actualItems);
            var enumerator = list.GetEnumerator(); 
            Assert.False(list.InitializeMethodCalled);
        }
    }
}