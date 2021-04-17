using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

namespace AsmResolver.Workspaces
{
    /// <summary>
    /// Represents a single node in a <see cref="WorkspaceIndex"/>.
    /// </summary>
    public class WorkspaceIndexNode
    {
        private readonly Dictionary<ObjectRelation, ISet<WorkspaceIndexNode>> _neighbors = new();
        private List<object>? _data = null;

        /// <summary>
        /// Creates a new instance of the <see cref="WorkspaceIndexNode"/> class.
        /// </summary>
        /// <param name="subject">The subject that is indexed.</param>
        public WorkspaceIndexNode(object subject)
        {
            Subject = subject;
        }

        private IList<object> Data
        {
            get
            {
                if (_data is null)
                    Interlocked.CompareExchange(ref _data, new(), null);
                return _data;
            }
        }

        /// <summary>
        /// Gets the subject associated to the node.
        /// </summary>
        public object Subject
        {
            get;
        }

        /// <summary>
        /// Registers a relation between two objects.
        /// </summary>
        /// <param name="relation">The relation.</param>
        /// <param name="node">The node representing the other object.</param>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TTarget">The type of object to relate to.</typeparam>
        public void AddRelation<TSource, TTarget>(ObjectRelation<TSource, TTarget> relation, WorkspaceIndexNode node)
        {
            if (!_neighbors.TryGetValue(relation, out var neighbors))
            {
                neighbors = new HashSet<WorkspaceIndexNode>();
                _neighbors.Add(relation, neighbors);
            }

            neighbors.Add(node);
        }

        /// <summary>
        /// Gets a collection of all nodes that are related to this object.
        /// </summary>
        public IEnumerable<WorkspaceIndexNode> GetAllRelatedNodes() => _neighbors.Values.SelectMany(x => x);

        /// <summary>
        /// Gets a collection of all nodes that are related to this object.
        /// <param name="exclusions">The relations that will be skipped.</param>
        /// </summary>
        public IEnumerable<WorkspaceIndexNode> GetAllRelatedNodes(params ObjectRelation[] exclusions)
        {
            return _neighbors
                .Select(n=> n.Key)
                .Where(r => !exclusions.Contains(r))
                .SelectMany(GetRelatedNodes);
        }


        /// <summary>
        /// Gets a collection of all nodes that are related to this object.
        /// </summary>
        public IEnumerable<object> GetAllRelatedObjects()
        {
            return GetAllRelatedNodes()
                .Select(x => x.Subject)
                .Distinct();
        }


        /// <summary>
        /// Gets a collection of all nodes that are related to this object.
        /// <param name="blocked">The relations that will be skipped.</param>
        /// </summary>
        public IEnumerable<object> GetAllRelatedObjects(params ObjectRelation[] blocked)
        {
            return GetAllRelatedNodes(blocked)
                .Select(x => x.Subject)
                .Distinct();
        }

        /// <summary>
        /// Gets a collection of all nodes that are related to this object of a given relation type.
        /// </summary>
        /// <param name="relation">The relation.</param>
        public IEnumerable<WorkspaceIndexNode> GetRelatedNodes(ObjectRelation relation)
        {
            return _neighbors.TryGetValue(relation, out var neighbors)
                ? neighbors
                : Enumerable.Empty<WorkspaceIndexNode>();
        }

        /// <summary>
        /// Gets a collection of all nodes that are related to these relations.
        /// </summary>
        /// <param name="relations">The relations.</param>
        public IEnumerable<WorkspaceIndexNode> GetRelatedNodes(params ObjectRelation[] relations)
        {
            return relations.Length != 0
                ? relations.SelectMany(GetRelatedNodes)
                : Enumerable.Empty<WorkspaceIndexNode>();
        }

        /// <summary>
        /// Gets a collection of all objects that are related to this object of a given relation type.
        /// </summary>
        /// <param name="relation">The relation.</param>
        /// <typeparam name="TSource">The type of the source object.</typeparam>
        /// <typeparam name="TTarget">The type of object to obtain.</typeparam>
        public IEnumerable<TTarget> GetRelatedObjects<TSource, TTarget>(ObjectRelation<TSource, TTarget> relation)
        {
            return GetRelatedNodes(relation)
                .Select(n => (TTarget) n.Subject)
                .Distinct();
        }

        /// <summary>
        /// Gets a collection of all objects that are related to these relations.
        /// </summary>
        /// <param name="relations">The relations.</param>
        public IEnumerable<object> GetRelatedObjects(params ObjectRelation[] relations)
        {
            return GetRelatedNodes(relations)
                .Select(n => n.Subject)
                .Distinct();
        }

        /// <summary>
        /// Gets a stored data of type <see cref="T"/>.
        /// </summary>
        /// <returns>default value if data with type <see cref="T"/> is not stored, otherwise the data.</returns>
        /// <typeparam name="T">The type of data to obtain.</typeparam>
        public T? GetData<T>() => Data
            .OfType<T>()
            .FirstOrDefault();

        /// <summary>
        /// Gets a stored data of type <see cref="T"/>.
        /// </summary>
        /// <param name="data">The data</param>
        /// <returns>false if data with type <see cref="T"/> is not stored, otherwise true.</returns>
        /// <typeparam name="T">The type of data to obtain.</typeparam>
        public bool TryGetData<T>([NotNullWhen(true)] out T? data)
        {
            for (int i = 0; i < Data.Count; i++)
            {
                if (Data[i] is not T newData)
                    continue;
                data = newData;
                return true;
            }
            data = default;
            return false;
        }

        /// <summary>
        /// Gets a stored data or creates new data of type <see cref="T"/>.
        /// </summary>
        /// <param name="defaultValue">Default value of new data.</param>
        /// <returns>The data.</returns>
        /// <typeparam name="T">The type of data to obtain.</typeparam>
        public T GetOrCreate<T>(T defaultValue)
        {
            if (!TryGetData<T>(out var data))
            {
                data = defaultValue;
                Data.Add(data!);
            }
            return data;
        }

        /// <summary>
        /// Gets a stored data or creates new data of type <see cref="T"/>.
        /// </summary>
        /// <param name="factory">Function that generates new data.</param>
        /// <returns>The data.</returns>
        /// <typeparam name="T">The type of data to obtain.</typeparam>
        public T GetOrCreate<T>(Func<T> factory)
        {
            if (!TryGetData<T>(out var data))
            {
                data = factory();
                Data.Add(data!);
            }
            return data;
        }

        /// <summary>
        /// Gets a stored data or creates new data of type <see cref="T"/>.
        /// </summary>
        /// <returns>The data.</returns>
        /// <typeparam name="T">The type of data to obtain.</typeparam>
        public T GetOrCreate<T>() where T : new()
        {
            if (!TryGetData<T>(out var data))
            {
                data = new();
                Data.Add(data);
            }
            return data;
        }

        /// <summary>
        /// Stores the data of type <see cref="T"/>.
        /// </summary>
        /// <param name="data">The data to store</param>
        /// <typeparam name="T">The type of data to store.</typeparam>
        public void SetData<T>(T data)
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data));

            if (TryGetData<T>(out var old))
                Data.Remove(old);
            Data.Add(data);
        }

        /// <summary>
        /// Removes data of type <see cref="T"/>.
        /// </summary>
        /// <returns>true if data with type <see cref="T"/> was removed, otherwise false.</returns>
        /// <typeparam name="T">The type of data to obtain.</typeparam>
        public bool RemoveData<T>()
            => TryGetData<T>(out var data)
               && Data.Remove(data);
    }
}
