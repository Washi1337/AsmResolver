using System;
using System.Collections.Generic;
using System.Linq;

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

        /// <summary>
        /// Gets a collection of all objects that are related to this object of a given relation type.
        /// </summary>
        /// <param name="relation">The relation.</param>
        /// <typeparam name="TSource">The type of the source object.</typeparam>
        /// <typeparam name="TTarget">The type of object to obtain.</typeparam>
        public IEnumerable<TSource> GetObjects<TSource, TTarget>(ObjectRelation<TSource, TTarget> relation)
        {
            return GetEdges(relation)
                .Select(e => (TSource) GetAdjacentNode(e).Subject)
                .Distinct();
        }

        /// <summary>
        /// Gets a collection of all objects that are related to this object of any of the given relation types.
        /// </summary>
        /// <param name="relations">The relations to include in the lookup.</param>
        public IEnumerable<object> GetObjects(params ObjectRelation[] relations)
        {
            return GetEdges(relations)
                .Select(n => GetAdjacentNode(n).Subject)
                .Distinct();
        }
    }
}
