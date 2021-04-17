using System;

namespace AsmResolver.Workspaces
{
    public readonly struct WorkspaceIndexEdge : IEquatable<WorkspaceIndexEdge>
    {
        public WorkspaceIndexEdge(WorkspaceIndexNode source, WorkspaceIndexNode target, ObjectRelation relation)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Target = target ?? throw new ArgumentNullException(nameof(target));
            Relation = relation ?? throw new ArgumentNullException(nameof(relation));
        }

        public WorkspaceIndexNode Source
        {
            get;
        }

        public WorkspaceIndexNode Target
        {
            get;
        }

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
