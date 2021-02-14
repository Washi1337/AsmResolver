using System;
using System.Collections.Generic;
using System.Linq;

namespace AsmResolver.Workspaces
{
    /// <summary>
    /// Represents a single node in a <see cref="WorkspaceIndex"/>.
    /// </summary>
    public class WorkspaceIndexNode
    {
        private readonly Dictionary<Type, ISet<WorkspaceIndexNode>> _neighbors = new();

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
        /// <param name="node">The node representing the other object.</param>
        /// <typeparam name="TRelation">The type of relation to register.</typeparam>
        public void AddRelation<TRelation>(WorkspaceIndexNode node)
        {
            var type = typeof(TRelation);
            if (!_neighbors.TryGetValue(type, out var neighbors))
            {
                neighbors = new HashSet<WorkspaceIndexNode>();
                _neighbors.Add(type, neighbors);
            }

            neighbors.Add(node);
        }

        /// <summary>
        /// Gets a collection of all nodes that are related to this object.
        /// </summary>
        public IEnumerable<WorkspaceIndexNode> GetAllRelatedObjects()
        {
            return _neighbors.Values
                .SelectMany(x => x)
                .Distinct();
        }

        /// <summary>
        /// Gets a collection of all nodes that are related to this object of a given relation type.
        /// </summary>
        /// <typeparam name="TRelation">The type of relation.</typeparam>
        public IEnumerable<WorkspaceIndexNode> GetRelatedObjects<TRelation>()
            where TRelation : IObjectRelation
        {
            return _neighbors.TryGetValue(typeof(TRelation), out var neighbors)
                ? neighbors
                : Enumerable.Empty<WorkspaceIndexNode>();
        }
    }
}
