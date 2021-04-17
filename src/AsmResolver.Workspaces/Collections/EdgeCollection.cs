using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AsmResolver.Workspaces.Collections
{
    /// <summary>
    /// Represents a collection of edges in a workspace index graph.
    /// </summary>
    public abstract class EdgeCollection : ICollection<WorkspaceIndexEdge>
    {
        private readonly Dictionary<ObjectRelation, HashSet<WorkspaceIndexEdge>> _entries = new();

        /// <summary>
        /// Initializes a new empty edge collection.
        /// </summary>
        /// <param name="owner">The owner of the collection.</param>
        protected internal EdgeCollection(WorkspaceIndexNode owner)
        {
            Owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        /// <summary>
        /// Gets the owner of this edge collection.
        /// </summary>
        public WorkspaceIndexNode Owner
        {
            get;
        }

        /// <inheritdoc />
        public int Count => _entries.Values.Sum(e => e.Count);

        /// <inheritdoc />
        public bool IsReadOnly => false;

        /// <summary>
        /// Validates the provided edge for validity in the context of this edge collection.
        /// </summary>
        /// <param name="edge">The edge to validate.</param>
        protected virtual void AssertEdgeValidity(in WorkspaceIndexEdge edge)
        {
            object source = edge.Source.Subject;
            object target = edge.Target.Subject;

            if (!edge.Relation.CanRelateObjects(source, target))
                throw new ArgumentException($"{source} cannot be related to {target} in the context of a {edge.Relation.Name} relation.");
        }

        /// <summary>
        /// Gets the node that the node is adjacent to.
        /// </summary>
        /// <param name="edge">The edge.</param>
        /// <returns>The adjacent node.</returns>
        protected abstract WorkspaceIndexNode GetAdjacentNode(in WorkspaceIndexEdge edge);

        /// <summary>
        /// Adds a single edge to the collection.
        /// </summary>
        /// <param name="item">The edge to add.</param>
        /// <returns><c>true</c> if the edge was added, <c>false</c> if the edge already existed.</returns>
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

        /// <summary>
        /// Gets a collection of all edges that are not related in the context of any of the provided relations.
        /// </summary>
        /// <param name="exclusions">The excluded relations.</param>
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
                .Select(e => GetAdjacentNode(e))
                .Distinct();
        }

        /// <summary>
        /// Gets a collection of all nodes that are related in the context of the provided relation.
        /// </summary>
        /// <param name="relation">The relation.</param>
        public IEnumerable<WorkspaceIndexNode> GetNodes(ObjectRelation relation)
        {
            return GetEdges(relation)
                .Select(e => GetAdjacentNode(e))
                .Distinct();
        }

        /// <summary>
        /// Gets a collection of all nodes that are related in the context of any of the provided relations.
        /// </summary>
        /// <param name="relations">The relations.</param>
        public IEnumerable<WorkspaceIndexNode> GetNodes(params ObjectRelation[] relations)
        {
            return GetEdges(relations)
                .Select(e => GetAdjacentNode(e))
                .Distinct();
        }

        /// <summary>
        /// Gets a collection of all nodes that are related to this object.
        /// </summary>
        public IEnumerable<WorkspaceIndexNode> GetAllNodes()
        {
            return _entries
                .SelectMany(entry => entry.Value, (_, edge) => GetAdjacentNode(edge))
                .Distinct();
        }

        /// <summary>
        /// Gets a collection of all nodes that are related to this object.
        /// <param name="exclusions">The relations that will be skipped.</param>
        /// </summary>
        public IEnumerable<WorkspaceIndexNode> GetAllNodes(params ObjectRelation[] exclusions)
        {
            return GetAllEdges(exclusions)
                .Select(e => GetAdjacentNode(e))
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

        /// <summary>
        /// Gets a collection of all nodes that are related to this object.
        /// </summary>
        public IEnumerable<object> GetAllObjects()
        {
            return this
                .Select(e => GetAdjacentNode(e).Subject)
                .Distinct();
        }

        /// <summary>
        /// Gets a collection of all nodes that are related to this object.
        /// <param name="exclusions">The relations that will be skipped.</param>
        /// </summary>
        public IEnumerable<object> GetAllObjects(params ObjectRelation[] exclusions)
        {
            return GetAllEdges(exclusions)
                .Select(e => GetAdjacentNode(e).Subject)
                .Distinct();
        }

        /// <summary>
        /// Gets an enumerator that enumerates all edges in the collection.
        /// </summary>
        /// <returns>The enumerator.</returns>
        public Enumerator GetEnumerator() => new(this);

        /// <inheritdoc />
        IEnumerator<WorkspaceIndexEdge> IEnumerable<WorkspaceIndexEdge>.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Enumerates all edges in an <see cref="EdgeCollection"/>.
        /// </summary>
        public struct Enumerator : IEnumerator<WorkspaceIndexEdge>
        {
            private Dictionary<ObjectRelation,HashSet<WorkspaceIndexEdge>>.Enumerator _entriesEnumerator;
            private HashSet<WorkspaceIndexEdge>.Enumerator _edgeEnumerator;
            private bool _initialized;

            internal Enumerator(EdgeCollection collection)
            {
                _entriesEnumerator = collection._entries.GetEnumerator();
                _initialized = false;
            }

            /// <inheritdoc />
            public WorkspaceIndexEdge Current => _initialized ? _edgeEnumerator.Current : default;

            /// <inheritdoc />
            object IEnumerator.Current => Current;

            /// <inheritdoc />
            public bool MoveNext()
            {
                if (_initialized && _edgeEnumerator.MoveNext())
                    return true;

                while (_entriesEnumerator.MoveNext())
                {
                    if (_initialized)
                        _edgeEnumerator.Dispose();

                    _edgeEnumerator = _entriesEnumerator.Current.Value.GetEnumerator();
                    _initialized = true;

                    if (_edgeEnumerator.MoveNext())
                        return true;
                }

                return false;
            }

            /// <inheritdoc />
            public void Reset() => throw new InvalidOperationException();

            /// <inheritdoc />
            public void Dispose()
            {
                _entriesEnumerator.Dispose();

                if (_initialized)
                    _edgeEnumerator.Dispose();
            }
        }
    }
}
