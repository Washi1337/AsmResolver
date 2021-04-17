using System;

namespace AsmResolver.Workspaces.Collections
{
    public sealed class IncomingEdgesCollection : EdgeCollection
    {
        /// <inheritdoc />
        public IncomingEdgesCollection(WorkspaceIndexNode owner)
            : base(owner)
        {
        }

        /// <inheritdoc />
        protected override void AssertEdgeValidity(in WorkspaceIndexEdge edge)
        {
            base.AssertEdgeValidity(in edge);
            if (edge.Target != Owner)
                throw new ArgumentException("The edge targets a different node.");
        }

        /// <inheritdoc />
        protected override WorkspaceIndexNode GetNode(in WorkspaceIndexEdge edge) => edge.Source;

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
