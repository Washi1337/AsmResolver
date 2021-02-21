using System.Collections.Generic;
using System.Linq;

namespace AsmResolver.Workspaces
{
    /// <summary>
    /// Represents a single node in a <see cref="WorkspaceIndex"/>.
    /// </summary>
    public class WorkspaceIndexNode
    {
        private readonly Dictionary<ObjectRelation, ISet<WorkspaceIndexNode>> _neighbors = new();

        /// <summary>
        /// Creates a new instance of the <see cref="WorkspaceIndexNode"/> class.
        /// </summary>
        /// <param name="subject">The subject that is indexed.</param>
        public WorkspaceIndexNode(object subject)
        {
            Subject = subject;
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
    }
}
