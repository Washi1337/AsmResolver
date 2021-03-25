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
        /// <typeparam name="T">The type of object to relate to.</typeparam>
        public void AddRelation<T>(ObjectRelation<T> relation, WorkspaceIndexNode node)
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
        /// </summary>
        public IEnumerable<object> GetAllRelatedObjects()
        {
            return GetAllRelatedNodes()
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
        /// Gets a collection of all objects that are related to this object of a given relation type.
        /// </summary>
        /// <param name="relation">The relation.</param>
        /// <typeparam name="T">The type of object to obtain.</typeparam>
        public IEnumerable<T> GetRelatedObjects<T>(ObjectRelation<T> relation)
        {
            return GetRelatedNodes(relation)
                .Select(n => (T) n.Subject)
                .Distinct();
        }

        /// <summary>
        /// Gets a stored data of type <see cref="T"/>.
        /// </summary>
        /// <param name="relation">The relation.</param>
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
        public bool TryGetData<T>([NotNullWhen(true)]out T? data)
        {
            for (int i = 0; i < Data.Count; i++)
            {
                if(Data[i] is not T newData)
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
            if (TryGetData<T>(out var old))
                Data.Remove(old);
            Data.Add(data);
        }
    }
}
