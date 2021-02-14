using System.Collections.Generic;

namespace AsmResolver.Workspaces
{
    /// <summary>
    /// Provides a database in the form of a graph, containing all the indexed objects in the workspace.
    /// </summary>
    public class WorkspaceIndex
    {
        private readonly Dictionary<object, WorkspaceIndexNode> _nodes = new();

        /// <summary>
        /// Gets a collection of all indexed objects.
        /// </summary>
        public IEnumerable<WorkspaceIndexNode> GetAllNodes() => _nodes.Values;

        /// <summary>
        /// Gets or creates a node in the database representing the provided object.
        /// </summary>
        /// <param name="subject">The object to index.</param>
        /// <returns>The node associated to the subject.</returns>
        public WorkspaceIndexNode GetOrCreateNode(object subject)
        {
            if (!_nodes.TryGetValue(subject, out var node))
            {
                node = new WorkspaceIndexNode(subject);
                _nodes.Add(subject, node);
            }

            return node;
        }
    }
}
