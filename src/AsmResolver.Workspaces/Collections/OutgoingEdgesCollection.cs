using System;
using System.Collections.Generic;
using System.Linq;

namespace AsmResolver.Workspaces.Collections
{
    /// <summary>
    /// Represents a collection of edges originating from a single <see cref="WorkspaceIndexNode"/>.
    /// </summary>
    public class OutgoingEdgesCollection : EdgeCollection
    {
        internal OutgoingEdgesCollection(WorkspaceIndexNode owner)
            : base(owner)
        {
        }

        /// <summary>
        /// Registers a relation between two objects.
        /// </summary>
        /// <param name="relation">The relation.</param>
        /// <param name="node">The node representing the other object.</param>
        /// <returns><c>true</c> if the relation did not exist and was added, <c>false</c> otherwise.</returns>
        public bool Add(ObjectRelation relation, WorkspaceIndexNode node)
        {
            return Add(new WorkspaceIndexEdge(Owner, node, relation));
        }

        /// <inheritdoc />
        public override bool Add(WorkspaceIndexEdge item)
        {
            if (base.Add(item))
            {
                item.Target.BackwardRelations.Add(item);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes a relation between two objects.
        /// </summary>
        /// <param name="relation">The relation.</param>
        /// <param name="node">The node representing the other object.</param>
        /// <returns><c>true</c> if the relation existed and was removed, <c>false</c> otherwise.</returns>
        public bool Remove(ObjectRelation relation, WorkspaceIndexNode node)
        {
            return Remove(new WorkspaceIndexEdge(Owner, node, relation));
        }

        /// <inheritdoc />
        public override bool Remove(WorkspaceIndexEdge item)
        {
            if (base.Remove(item))
            {
                item.Target.BackwardRelations.Remove(item);
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        protected override void AssertEdgeValidity(in WorkspaceIndexEdge edge)
        {
            if (edge.Source != Owner)
                throw new ArgumentException("The edge originates from a different node.");
            base.AssertEdgeValidity(in edge);
        }

        /// <inheritdoc />
        protected override WorkspaceIndexNode GetAdjacentNode(in WorkspaceIndexEdge edge) => edge.Target;

        /// <summary>
        /// Gets a collection of all objects that are related to this object of a given relation type.
        /// </summary>
        /// <param name="relation">The relation.</param>
        /// <typeparam name="TSource">The type of the source object.</typeparam>
        /// <typeparam name="TTarget">The type of object to obtain.</typeparam>
        public IEnumerable<TTarget> GetObjects<TSource, TTarget>(ObjectRelation<TSource, TTarget> relation)
        {
            return GetEdges(relation)
                .Select(e => (TTarget) GetAdjacentNode(e).Subject)
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
