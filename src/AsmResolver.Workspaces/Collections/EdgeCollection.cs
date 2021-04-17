using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AsmResolver.Workspaces.Collections
{
    public abstract class EdgeCollection : ICollection<WorkspaceIndexEdge>
    {
        private readonly Dictionary<ObjectRelation, HashSet<WorkspaceIndexEdge>> _entries = new();

        protected EdgeCollection(WorkspaceIndexNode owner)
        {
            Owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        /// <inheritdoc />
        public int Count => _entries.Values.Sum(e => e.Count);

        /// <inheritdoc />
        public bool IsReadOnly => false;

        public WorkspaceIndexNode Owner
        {
            get;
        }

        protected virtual void AssertEdgeValidity(in WorkspaceIndexEdge edge)
        {
        }

        protected abstract WorkspaceIndexNode GetNode(in WorkspaceIndexEdge edge);

        public virtual bool Add(WorkspaceIndexEdge item)
        {
            AssertEdgeValidity(item);

            if (!_entries.TryGetValue(item.Relation, out var neighbors))
            {
                neighbors = new HashSet<WorkspaceIndexEdge>();
                _entries.Add(item.Relation, neighbors);
            }

            return neighbors.Add(item);
        }

        /// <inheritdoc />
        void ICollection<WorkspaceIndexEdge>.Add(WorkspaceIndexEdge item) => Add(item);

        /// <inheritdoc />
        public void Clear()
        {
            foreach (var item in this.ToArray())
                Remove(item);
        }

        /// <inheritdoc />
        public bool Contains(WorkspaceIndexEdge item)
        {
            return _entries.TryGetValue(item.Relation, out var neighbors) && neighbors.Contains(item);
        }

        /// <inheritdoc />
        public void CopyTo(WorkspaceIndexEdge[] array, int arrayIndex)
        {
            if (array is null)
                throw new ArgumentNullException(nameof(array));
            if (arrayIndex <= 0)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));

            int count = Count;
            if (array.Length - arrayIndex < count)
                throw new ArgumentException("Not enough space in the target array.");

            foreach (var set in _entries.Values)
            {
                set.CopyTo(array, arrayIndex);
                arrayIndex += set.Count;
            }
        }

        /// <inheritdoc />
        public bool Remove(WorkspaceIndexEdge item)
        {
            return _entries.TryGetValue(item.Relation, out var neighbors) && neighbors.Remove(item);
        }

        /// <summary>
        /// Gets a collection of all edges that are related in the context of any of the provided relation.
        /// </summary>
        /// <param name="relation">The relation.</param>
        public IEnumerable<WorkspaceIndexEdge> GetEdges(ObjectRelation relation)
        {
            return _entries.TryGetValue(relation, out var set)
                ? set
                : Enumerable.Empty<WorkspaceIndexEdge>();
        }

        /// <summary>
        /// Gets a collection of all edges that are related in the context of any of the provided relations.
        /// </summary>
        /// <param name="relations">The relations.</param>
        public IEnumerable<WorkspaceIndexEdge> GetEdges(params ObjectRelation[] relations)
        {
            foreach (var relation in relations)
            {
                if (_entries.TryGetValue(relation, out var set))
                {
                    foreach (var edge in set)
                        yield return edge;
                }
            }
        }

        public IEnumerable<WorkspaceIndexEdge> GetAllEdges(params ObjectRelation[] exclusions)
        {
            foreach (var entry in _entries)
            {
                if (exclusions.Contains(entry.Key))
                    continue;

                foreach (var edge in entry.Value)
                    yield return edge;
            }
        }

        /// <summary>
        /// Gets a collection of nodes that are related to this object.
        /// </summary>
        public IEnumerable<WorkspaceIndexNode> GetNodes()
        {
            return this
                .Select(e => GetNode(e))
                .Distinct();
        }

        /// <summary>
        /// Gets a collection of all nodes that are related in the context of the provided relation.
        /// </summary>
        /// <param name="relation">The relation.</param>
        public IEnumerable<WorkspaceIndexNode> GetNodes(ObjectRelation relation)
        {
            return GetEdges(relation)
                .Select(e => GetNode(e))
                .Distinct();
        }

        /// <summary>
        /// Gets a collection of all nodes that are related in the context of any of the provided relations.
        /// </summary>
        /// <param name="relations">The relations.</param>
        public IEnumerable<WorkspaceIndexNode> GetNodes(params ObjectRelation[] relations)
        {
            return GetEdges(relations)
                .Select(e => GetNode(e))
                .Distinct();
        }

        /// <summary>
        /// Gets a collection of all nodes that are related to this object.
        /// </summary>
        public IEnumerable<WorkspaceIndexNode> GetAllNodes()
        {
            return _entries
                .SelectMany(entry => entry.Value, (_, edge) => GetNode(edge))
                .Distinct();
        }

        /// <summary>
        /// Gets a collection of all nodes that are related to this object.
        /// <param name="exclusions">The relations that will be skipped.</param>
        /// </summary>
        public IEnumerable<WorkspaceIndexNode> GetAllNodes(params ObjectRelation[] exclusions)
        {
            return GetAllEdges(exclusions)
                .Select(e => GetNode(e))
                .Distinct();
        }

        /// <summary>
        /// Gets a collection of all objects that are related to this object of a given relation type.
        /// </summary>
        /// <param name="relation">The relation.</param>
        /// <typeparam name="TSource">The type of the source object.</typeparam>
        /// <typeparam name="TTarget">The type of object to obtain.</typeparam>
        public IEnumerable<TTarget> GetObjects<TSource, TTarget>(ObjectRelation<TSource, TTarget> relation)
        {
            return GetEdges(relation).Select(e => (TTarget) GetNode(e).Subject);
        }

        /// <summary>
        /// Gets a collection of all objects that are related to this object of any of the given relation types.
        /// </summary>
        /// <param name="relations">The relations to include in the lookup.</param>
        public IEnumerable<object> GetObjects(params ObjectRelation[] relations)
        {
            return GetEdges(relations)
                .Select(n => GetNode(n).Subject)
                .Distinct();
        }

        /// <summary>
        /// Gets a collection of all nodes that are related to this object.
        /// </summary>
        public IEnumerable<object> GetAllObjects()
        {
            return this.Select(e => GetNode(e).Subject);
        }

        /// <summary>
        /// Gets a collection of all nodes that are related to this object.
        /// <param name="blocked">The relations that will be skipped.</param>
        /// </summary>
        public IEnumerable<object> GetAllObjects(params ObjectRelation[] exclusions)
        {
            return GetAllEdges(exclusions)
                .Select(e => GetNode(e).Subject)
                .Distinct();
        }

        /// <inheritdoc />
        public IEnumerator<WorkspaceIndexEdge> GetEnumerator()
        {
            return _entries.Values.SelectMany(x => x).GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }
}
