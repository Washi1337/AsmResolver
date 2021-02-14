using AsmResolver.Workspaces.Tests.Mock;
using Xunit;

namespace AsmResolver.Workspaces.Tests
{
    public class WorkspaceIndexNodeTest
    {
        private readonly WorkspaceIndex _index = new();

        [Fact]
        public void AddRelationShouldResultInRelatedObject()
        {
            object subject1 = new object();
            object subject2 = new object();
            var node1 = _index.GetOrCreateNode(subject1);
            var node2 = _index.GetOrCreateNode(subject2);

            node1.AddRelation(MockRelations.Relation1, node2);

            Assert.Contains(subject2, node1.GetRelatedObjects(MockRelations.Relation1));
        }

        [Fact]
        public void AddMultipleObjectsWithSameRelation()
        {
            object subject1 = new object();
            object subject2 = new object();
            object subject3 = new object();
            var node1 = _index.GetOrCreateNode(subject1);
            var node2 = _index.GetOrCreateNode(subject2);
            var node3 = _index.GetOrCreateNode(subject3);

            node1.AddRelation(MockRelations.Relation1, node2);
            node1.AddRelation(MockRelations.Relation1, node3);

            Assert.Contains(subject2, node1.GetRelatedObjects(MockRelations.Relation1));
            Assert.Contains(subject3, node1.GetRelatedObjects(MockRelations.Relation1));
        }

        [Fact]
        public void AddMultipleObjectsWithDifferentRelations()
        {
            object subject1 = new object();
            object subject2 = new object();
            object subject3 = new object();
            var node1 = _index.GetOrCreateNode(subject1);
            var node2 = _index.GetOrCreateNode(subject2);
            var node3 = _index.GetOrCreateNode(subject3);

            node1.AddRelation(MockRelations.Relation1, node2);
            node1.AddRelation(MockRelations.Relation2, node3);

            Assert.Contains(subject2, node1.GetRelatedObjects(MockRelations.Relation1));
            Assert.DoesNotContain(subject3, node1.GetRelatedObjects(MockRelations.Relation1));
            Assert.DoesNotContain(subject2, node1.GetRelatedObjects(MockRelations.Relation2));
            Assert.Contains(subject3, node1.GetRelatedObjects(MockRelations.Relation2));
        }
    }
}
