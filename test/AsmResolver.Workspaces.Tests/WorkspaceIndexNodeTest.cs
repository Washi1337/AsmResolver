using System;
using AsmResolver.Workspaces.Tests.Mock;
using Xunit;

namespace AsmResolver.Workspaces.Tests
{
    public class WorkspaceIndexNodeTest
    {
        private readonly WorkspaceIndex _index = new();

        [Fact]
        public void AddOutgoingRelationShouldResultInRelatedObject()
        {
            object subject1 = new object();
            object subject2 = new object();
            var node1 = _index.GetOrCreateNode(subject1);
            var node2 = _index.GetOrCreateNode(subject2);

            node1.ForwardRelations.Add(MockRelations.Relation1, node2);

            Assert.Contains(subject2, node1.ForwardRelations.GetObjects(MockRelations.Relation1));
            Assert.Contains(node2, node1.ForwardRelations.GetNodes(MockRelations.Relation1));
            Assert.Contains(node2, node1.ForwardRelations.GetNodes());
            Assert.Contains(subject1, node2.BackwardRelations.GetObjects(MockRelations.Relation1));
            Assert.Contains(node1, node2.BackwardRelations.GetNodes(MockRelations.Relation1));
            Assert.Contains(node1, node2.BackwardRelations.GetNodes());
        }

        [Fact]
        public void RemoveForwardRelationShouldRemoveBackwardRelationInTarget()
        {
            object subject1 = new object();
            object subject2 = new object();
            var node1 = _index.GetOrCreateNode(subject1);
            var node2 = _index.GetOrCreateNode(subject2);

            Assert.True(node1.ForwardRelations.Add(MockRelations.Relation1, node2));
            Assert.Single(node2.BackwardRelations);
            Assert.True(node1.ForwardRelations.Remove(MockRelations.Relation1, node2));
            Assert.Empty(node2.BackwardRelations);
        }

        [Fact]
        public void AddInvalidOutgoingRelationEdgeShouldThrow()
        {
            object subject1 = new object();
            object subject2 = new object();
            var node1 = _index.GetOrCreateNode(subject1);
            var node2 = _index.GetOrCreateNode(subject2);

            Assert.Throws<ArgumentException>(() =>
                node1.ForwardRelations.Add(new WorkspaceIndexEdge(node2, node1, MockRelations.Relation1)));
        }

        [Fact]
        public void AddInvalidIncomingRelationEdgeShouldThrow()
        {
            object subject1 = new object();
            object subject2 = new object();
            var node1 = _index.GetOrCreateNode(subject1);
            var node2 = _index.GetOrCreateNode(subject2);

            Assert.Throws<ArgumentException>(() =>
                node1.BackwardRelations.Add(new WorkspaceIndexEdge(node1, node2, MockRelations.Relation1)));
        }

        [Fact]
        public void AddInvalidRelationShouldThrow()
        {
            string subject1 = "abcdefg";
            object subject2 = new object();
            var node1 = _index.GetOrCreateNode(subject1);
            var node2 = _index.GetOrCreateNode(subject2);

            Assert.Throws<ArgumentException>(() => node1.ForwardRelations.Add(MockRelations.ObjectToStringRelation, node2));
        }

        [Fact]
        public void GetStronglyTypedOutgoingRelatedObjects()
        {
            object subject1 = new object();
            string subject2 = "abcdefg";
            var node1 = _index.GetOrCreateNode(subject1);
            var node2 = _index.GetOrCreateNode(subject2);

            node1.ForwardRelations.Add(MockRelations.ObjectToStringRelation, node2);
            Assert.Contains(subject2, node1.ForwardRelations.GetObjects(MockRelations.ObjectToStringRelation));
        }

        [Fact]
        public void GetStronglyTypedIncomingRelatedObjects()
        {
            string subject1 = "abcdefg";
            object subject2 = new object();
            var node1 = _index.GetOrCreateNode(subject1);
            var node2 = _index.GetOrCreateNode(subject2);

            node1.ForwardRelations.Add(MockRelations.StringToObjectRelation, node2);
            Assert.Contains(subject1, node2.BackwardRelations.GetObjects(MockRelations.StringToObjectRelation));
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

            node1.ForwardRelations.Add(MockRelations.Relation1, node2);
            node1.ForwardRelations.Add(MockRelations.Relation1, node3);

            Assert.Contains(subject2, node1.ForwardRelations.GetObjects(MockRelations.Relation1));
            Assert.Contains(subject3, node1.ForwardRelations.GetObjects(MockRelations.Relation1));

            Assert.Contains(node2, node1.ForwardRelations.GetNodes(MockRelations.Relation1));
            Assert.Contains(node3, node1.ForwardRelations.GetNodes(MockRelations.Relation1));
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

            node1.ForwardRelations.Add(MockRelations.Relation1, node2);
            node1.ForwardRelations.Add(MockRelations.Relation2, node3);

            Assert.Contains(subject2, node1.ForwardRelations.GetObjects(MockRelations.Relation1));
            Assert.DoesNotContain(subject3, node1.ForwardRelations.GetObjects(MockRelations.Relation1));
            Assert.DoesNotContain(subject2, node1.ForwardRelations.GetObjects(MockRelations.Relation2));
            Assert.Contains(subject3, node1.ForwardRelations.GetObjects(MockRelations.Relation2));

            Assert.Contains(node2, node1.ForwardRelations.GetNodes(MockRelations.Relation1));
            Assert.DoesNotContain(node3, node1.ForwardRelations.GetNodes(MockRelations.Relation1));
            Assert.DoesNotContain(node2, node1.ForwardRelations.GetNodes(MockRelations.Relation2));
            Assert.Contains(node3, node1.ForwardRelations.GetNodes(MockRelations.Relation2));
        }

        [Fact]
        public void MultipleRelatedNodes()
        {
            object subject1 = new object();
            object subject2 = new object();
            object subject3 = new object();
            object subject4 = new object();
            object subject5 = new object();
            var node1 = _index.GetOrCreateNode(subject1);
            var node2 = _index.GetOrCreateNode(subject2);
            var node3 = _index.GetOrCreateNode(subject3);
            var node4 = _index.GetOrCreateNode(subject4);
            var node5 = _index.GetOrCreateNode(subject5);

            node1.ForwardRelations.Add(MockRelations.Relation1, node2);
            node1.ForwardRelations.Add(MockRelations.Relation2, node3);
            node1.ForwardRelations.Add(MockRelations.Relation3, node4);
            node1.ForwardRelations.Add(MockRelations.Relation4, node5);

            Assert.Contains(subject2, node1.ForwardRelations.GetObjects(MockRelations.Relation1, MockRelations.Relation2));
            Assert.Contains(subject3, node1.ForwardRelations.GetObjects(MockRelations.Relation1, MockRelations.Relation2));
            Assert.DoesNotContain(subject4, node1.ForwardRelations.GetObjects(MockRelations.Relation1, MockRelations.Relation2));
            Assert.DoesNotContain(subject5, node1.ForwardRelations.GetObjects(MockRelations.Relation1, MockRelations.Relation2));

            Assert.Contains(node4, node1.ForwardRelations.GetNodes(MockRelations.Relation3, MockRelations.Relation4));
            Assert.Contains(node5, node1.ForwardRelations.GetNodes(MockRelations.Relation3, MockRelations.Relation4));
            Assert.DoesNotContain(node2, node1.ForwardRelations.GetNodes(MockRelations.Relation3, MockRelations.Relation4));
            Assert.DoesNotContain(node3, node1.ForwardRelations.GetNodes(MockRelations.Relation3, MockRelations.Relation4));
        }

        [Fact]
        public void MultipleRelatedNodesBlocked()
        {
            object subject1 = new object();
            object subject2 = new object();
            object subject3 = new object();
            object subject4 = new object();
            object subject5 = new object();
            var node1 = _index.GetOrCreateNode(subject1);
            var node2 = _index.GetOrCreateNode(subject2);
            var node3 = _index.GetOrCreateNode(subject3);
            var node4 = _index.GetOrCreateNode(subject4);
            var node5 = _index.GetOrCreateNode(subject5);

            node1.ForwardRelations.Add(MockRelations.Relation1, node2);
            node1.ForwardRelations.Add(MockRelations.Relation2, node3);
            node1.ForwardRelations.Add(MockRelations.Relation3, node4);
            node1.ForwardRelations.Add(MockRelations.Relation4, node5);

            Assert.DoesNotContain(subject2, node1.ForwardRelations.GetAllObjects(MockRelations.Relation1, MockRelations.Relation2));
            Assert.DoesNotContain(subject3, node1.ForwardRelations.GetAllObjects(MockRelations.Relation1, MockRelations.Relation2));
            Assert.Contains(subject4, node1.ForwardRelations.GetAllObjects(MockRelations.Relation1, MockRelations.Relation2));
            Assert.Contains(subject5, node1.ForwardRelations.GetAllObjects(MockRelations.Relation1, MockRelations.Relation2));

            Assert.DoesNotContain(node4, node1.ForwardRelations.GetAllNodes(MockRelations.Relation3, MockRelations.Relation4));
            Assert.DoesNotContain(node5, node1.ForwardRelations.GetAllNodes(MockRelations.Relation3, MockRelations.Relation4));
            Assert.Contains(node2, node1.ForwardRelations.GetAllNodes(MockRelations.Relation3, MockRelations.Relation4));
            Assert.Contains(node3, node1.ForwardRelations.GetAllNodes(MockRelations.Relation3, MockRelations.Relation4));
        }

    }
}
