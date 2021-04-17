using System;

namespace AsmResolver.Workspaces
{
    /// <summary>
    /// Describes a relation between two objects in a workspace.
    /// </summary>
    public abstract class ObjectRelation
    {
        /// <summary>
        /// Creates a new instance of the <see cref="ObjectRelation"/> class.
        /// </summary>
        /// <param name="name">The name of the relation.</param>
        /// <param name="guid">The unique identifier of the relation.</param>
        public ObjectRelation(string name, Guid guid)
        {
            Name = name;
            Guid = guid;
        }

        /// <summary>
        /// Gets the name of the relation.
        /// </summary>
        public string Name
        {
            get;
        }

        /// <summary>
        /// Get the unique identifier of the relation.
        /// </summary>
        public Guid Guid
        {
            get;
        }

        /// <summary>
        /// Determine whether two relations are considered equal.
        /// </summary>
        /// <param name="other">The other relation object.</param>
        protected bool Equals(ObjectRelation other) => Name == other.Name && Guid.Equals(other.Guid);

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            return obj.GetType() == GetType() && Equals((ObjectRelation) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode() => Guid.GetHashCode();
    }

    /// <summary>
    /// Describes a relation between two objects in a workspace.
    /// </summary>
    /// <typeparam name="TSource">The type of objects that this edge originates from.</typeparam>
    /// <typeparam name="TTarget">The type of objects that this relation connects to.</typeparam>
    public class ObjectRelation<TSource, TTarget> : ObjectRelation
    {
        /// <summary>
        /// Creates a new instance of the <see cref="ObjectRelation"/> class.
        /// </summary>
        /// <param name="name">The name of the relation.</param>
        /// <param name="guid">The unique identifier of the relation.</param>
        public ObjectRelation(string name, Guid guid)
            : base(name, guid)
        {
        }
    }
}
