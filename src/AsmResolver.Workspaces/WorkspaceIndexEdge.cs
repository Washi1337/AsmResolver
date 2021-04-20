using System;

namespace AsmResolver.Workspaces
{
    /// <summary>
    /// Represents a single edge in a workspace index graph.
    /// </summary>
    public readonly struct WorkspaceIndexEdge : IEquatable<WorkspaceIndexEdge>
    {
        /// <summary>
        /// Creates a new edge.
        /// </summary>
        /// <param name="source">The node the edge is originating from.</param>
        /// <param name="target">The node the edge is targeting.</param>
        /// <param name="relation">The type of relation the edge encodes.</param>
        public WorkspaceIndexEdge(WorkspaceIndexNode source, WorkspaceIndexNode target, ObjectRelation relation)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Target = target ?? throw new ArgumentNullException(nameof(target));
            Relation = relation ?? throw new ArgumentNullException(nameof(relation));
        }

        /// <summary>
        /// Gets the node the edge is originating from.
        /// </summary>
        public WorkspaceIndexNode Source
        {
            get;
        }

        /// <summary>
        /// Gets the node the edge is targeting.
        /// </summary>
        public WorkspaceIndexNode Target
        {
            get;
        }

        /// <summary>
        /// Gets the relation the edge encodes.
        /// </summary>
        public ObjectRelation Relation
        {
            get;
        }

        /// <inheritdoc />
        public bool Equals(WorkspaceIndexEdge other)
        {
            return Source.Equals(other.Source)
                   && Target.Equals(other.Target)
                   && Relation.Equals(other.Relation);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is WorkspaceIndexEdge edge && Equals(edge);

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Source.GetHashCode();
                hashCode = (hashCode * 397) ^ Target.GetHashCode();
                hashCode = (hashCode * 397) ^ Relation.GetHashCode();
                return hashCode;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Source.GetHashCode()} -> {Target.GetHashCode()} ({Relation.Name})";
        }
    }
}
