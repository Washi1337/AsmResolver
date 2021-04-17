using System;

namespace AsmResolver.Workspaces.Collections
{
    /// <summary>
    /// Represents a collection of edges targeting a single <see cref="WorkspaceIndexNode"/>.
    /// </summary>
    public sealed class IncomingEdgesCollection : EdgeCollection
    {
        internal IncomingEdgesCollection(WorkspaceIndexNode owner)
            : base(owner)
        {
        }

        /// <inheritdoc />
        protected override void AssertEdgeValidity(in WorkspaceIndexEdge edge)
        {
            if (edge.Target != Owner)
                throw new ArgumentException("The edge targets a different node.");
            base.AssertEdgeValidity(in edge);
        }

        /// <inheritdoc />
        protected override WorkspaceIndexNode GetAdjacentNode(in WorkspaceIndexEdge edge) => edge.Source;

        /// <inheritdoc />
        public override bool Add(WorkspaceIndexEdge item)
        {
            if (base.Add(item))
            {
                item.Source.OutgoingEdges.Add(item);
                return true;
            }

            return false;
        }
    }
}
