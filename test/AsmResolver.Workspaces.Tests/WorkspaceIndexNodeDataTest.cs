using AsmResolver.Workspaces.Tests.Mock;
using Xunit;

namespace AsmResolver.Workspaces.Tests
{
    public class WorkspaceIndexNodeDataTest
    {
        private readonly WorkspaceIndex _index = new();

        [Fact]
        public void GetDataEmptyStruct()
        {
            var subject = new object();
            var node = _index.GetOrCreateNode(subject);

            var data = node.GetData<int>();
            Assert.Equal(default, data);
        }

        [Fact]
        public void GetDataEmptyObject()
        {
            var subject = new object();
            var node = _index.GetOrCreateNode(subject);

            var data = node.GetData<object>();
            Assert.Null(data);
        }

        [Fact]
        public void GetDataNotEmpty()
        {
            var subject = new object();
            var node = _index.GetOrCreateNode(subject);

            node.SetData<int>(123);
            var data = node.GetData<int>();
            Assert.Equal(123,data);
        }

        [Fact]
        public void GetDataNotEmptyDifferentType()
        {
            var subject = new object();
            var node = _index.GetOrCreateNode(subject);

            node.SetData<float>(123.0f);
            var data = node.GetData<int>();
            Assert.Equal(default,data);
        }

        [Fact]
        public void TryGetDataEmptyStruct()
        {
            var subject = new object();
            var node = _index.GetOrCreateNode(subject);

            var result = node.TryGetData<int>(out var data);
            Assert.False(result);
            Assert.Equal(default, data);
        }

        [Fact]
        public void TryGetDataEmptyObject()
        {
            var subject = new object();
            var node = _index.GetOrCreateNode(subject);

            var result = node.TryGetData<object>(out var data);
            Assert.False(result);
            Assert.Null(data);
        }

        [Fact]
        public void TryGetDataNotEmpty()
        {
            var subject = new object();
            var node = _index.GetOrCreateNode(subject);

            node.SetData<int>(123);
            var result = node.TryGetData<int>(out var data);
            Assert.True(result);
            Assert.Equal(123,data);
        }

        [Fact]
        public void TryGetDataNotEmptyDifferentType()
        {
            var subject = new object();
            var node = _index.GetOrCreateNode(subject);

            node.SetData<float>(123.0f);
            var result = node.TryGetData<int>(out var data);
            Assert.False(result);
            Assert.Equal(default,data);
        }


        [Fact]
        public void GetOrCreateDataDefaultValueEmptyStruct()
        {
            var subject = new object();
            var node = _index.GetOrCreateNode(subject);

            var data = node.GetOrCreate<int>(1);
            Assert.Equal(1,data);
        }

        [Fact]
        public void GetOrCreateDataDefaultValueNotEmptyStruct()
        {
            var subject = new object();
            var node = _index.GetOrCreateNode(subject);

            node.SetData<int>(1);
            var data = node.GetOrCreate<int>(2);
            Assert.Equal(1,data);
        }

        [Fact]
        public void GetOrCreateDataDefaultValueEmptyObject()
        {
            var subject = new object();
            var node = _index.GetOrCreateNode(subject);

            var data = node.GetOrCreate<object>(new object());
            Assert.NotNull(data);
        }

        [Fact]
        public void GetOrCreateDataDefaultValueNotEmptyObject()
        {
            var subject = new object();
            var node = _index.GetOrCreateNode(subject);

            node.SetData<object>((object)1);
            var data = node.GetOrCreate<object>(new object());
            Assert.Equal((object)1,data);
        }

        [Fact]
        public void GetOrCreateDataFactoryValueEmptyStruct()
        {
            var subject = new object();
            var node = _index.GetOrCreateNode(subject);

            var data = node.GetOrCreate<int>(()=> 2);
            Assert.Equal(2,data);
        }

        [Fact]
        public void GetOrCreateDataFactoryValueNotEmptyStruct()
        {
            var subject = new object();
            var node = _index.GetOrCreateNode(subject);

            node.SetData<int>(2);
            var data = node.GetOrCreate<int>(() => 3);
            Assert.Equal(2,data);
        }

        [Fact]
        public void GetOrCreateDataFactoryValueEmptyObject()
        {
            var subject = new object();
            var node = _index.GetOrCreateNode(subject);

            var data = node.GetOrCreate<object>(() => new object());
            Assert.NotNull(data);
        }

        [Fact]
        public void GetOrCreateDataFactoryValueNotEmptyObject()
        {
            var subject = new object();
            var node = _index.GetOrCreateNode(subject);

            node.SetData<object>((object)2);
            var data = node.GetOrCreate<object>(() => new object());
            Assert.Equal((object)2,data);
        }

        [Fact]
        public void GetOrCreateDataNewValueEmptyObject()
        {
            var subject = new object();
            var node = _index.GetOrCreateNode(subject);

            var data = node.GetOrCreate<object>();
            Assert.NotNull(data);
        }

        [Fact]
        public void GetOrCreateDataNewValueNotEmptyObject()
        {
            var subject = new object();
            var node = _index.GetOrCreateNode(subject);

            node.SetData<object>((object)1);
            var data = node.GetOrCreate<object>();
            Assert.Equal((object)1,data);
        }

        [Fact]
        public void SetDataEmptyStruct()
        {
            var subject = new object();
            var node = _index.GetOrCreateNode(subject);

            node.SetData<int>(1);
            Assert.Equal(1, node.GetData<int>());
        }

        [Fact]
        public void SetDataEmptyObject()
        {
            var subject = new object();
            var node = _index.GetOrCreateNode(subject);

            node.SetData<object>(new ());
            Assert.NotNull(node.GetData<int>());
        }
        [Fact]
        public void SetDataRewriteEmptyStruct()
        {
            var subject = new object();
            var node = _index.GetOrCreateNode(subject);

            node.SetData<int>(1);
            node.SetData<int>(2);
            Assert.Equal(2, node.GetData<int>());
        }

        [Fact]
        public void SetDataRewriteEmptyObject()
        {
            var subject = new object();
            var node = _index.GetOrCreateNode(subject);

            node.SetData<object>(new ());
            node.SetData<object>((object)1);
            Assert.Equal((object)1,node.GetData<int>());
        }

        [Fact]
        public void RemoveDataEmptyStruct()
        {
            var subject = new object();
            var node = _index.GetOrCreateNode(subject);

            Assert.False(node.RemoveData<int>());
        }

        [Fact]
        public void RemoveDataEmptyObject()
        {
            var subject = new object();
            var node = _index.GetOrCreateNode(subject);

            Assert.False(node.RemoveData<object>());
        }

        [Fact]
        public void RemoveDataNotEmptyStruct()
        {
            var subject = new object();
            var node = _index.GetOrCreateNode(subject);

            node.SetData<int>(1);
            Assert.True(node.RemoveData<int>());
            Assert.False(node.TryGetData<int>(out _));
        }

        [Fact]
        public void RemoveDataNotEmptyObject()
        {
            var subject = new object();
            var node = _index.GetOrCreateNode(subject);

            node.SetData<object>(new ());
            Assert.True(node.RemoveData<object>());
            Assert.False(node.TryGetData<object>(out _));
        }
    }
}
