using Xunit;

namespace AsmResolver.Workspaces.Tests
{
    public class WorkspaceIndexTest
    {
        private readonly WorkspaceIndex _index = new();

        [Fact]
        public void AddNewObjectToIndexShouldCreateNode()
        {
            object subject = new object();
            var node = _index.GetOrCreateNode(subject);
            Assert.Same(subject, node.Subject);
            Assert.Contains(_index.GetAllNodes(), n => n.Subject == subject);
        }

        [Fact]
        public void AddSameObjectShouldReturnSameNode()
        {
            object subject = new object();
            var node1 = _index.GetOrCreateNode(subject);
            var node2 = _index.GetOrCreateNode(subject);
            Assert.Same(node1, node2);
        }

        [Fact]
        public void AddMultipleObjects()
        {
            object subject1 = new object();
            object subject2 = new object();
            var node1 = _index.GetOrCreateNode(subject1);
            var node2 = _index.GetOrCreateNode(subject2);

            Assert.NotSame(node1, node2);
        }
    }
}
