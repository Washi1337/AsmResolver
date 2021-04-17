using System;

namespace AsmResolver.Workspaces.Collections
{
    public class OutgoingEdgesCollection : EdgeCollection
    {
        public OutgoingEdgesCollection(WorkspaceIndexNode owner)
            : base(owner)
        {
        }

        /// <summary>
        /// Registers a relation between two objects.
        /// </summary>
        /// <param name="relation">The relation.</param>
        /// <param name="node">The node representing the other object.</param>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TTarget">The type of object to relate to.</typeparam>
        public bool Add<TSource, TTarget>(ObjectRelation<TSource, TTarget> relation, WorkspaceIndexNode node)
        {
            return Add(new WorkspaceIndexEdge(Owner, node, relation));
        }

        /// <inheritdoc />
        protected override void AssertEdgeValidity(in WorkspaceIndexEdge edge)
        {
            base.AssertEdgeValidity(in edge);
            if (edge.Source != Owner)
                throw new ArgumentException("The edge originates from a different node.");
        }

        /// <inheritdoc />
        protected override WorkspaceIndexNode GetNode(in WorkspaceIndexEdge edge) => edge.Target;

        /// <inheritdoc />
        public override bool Add(WorkspaceIndexEdge item)
        {
            if (base.Add(item))
            {
                item.Target.IncomingEdges.Add(item);
                return true;
            }

            return false;
        }
    }
}
