using System;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Defines an explicit implementation of a method defined by an interface.
    /// </summary>
    public readonly struct MethodImplementation : IEquatable<MethodImplementation>
    {
        /// <summary>
        /// Creates a new explicit implementation of a method.
        /// </summary>
        /// <param name="declaration">The interface method that is implemented.</param>
        /// <param name="body">The method implementing the base method.</param>
        public MethodImplementation(IMethodDefOrRef? declaration, IMethodDefOrRef? body)
        {
            Declaration = declaration;
            Body = body;
        }

        /// <summary>
        /// Gets the interface method that is implemented.
        /// </summary>
        public IMethodDefOrRef? Declaration
        {
            get;
        }

        /// <summary>
        /// Gets the method that implements the base method.
        /// </summary>
        public IMethodDefOrRef? Body
        {
            get;
        }

        /// <inheritdoc />
        public override string ToString() => $".override {Declaration} with {Body}";

        /// <summary>
        /// Determines whether two method implementations record are equal.
        /// </summary>
        /// <param name="other">The other implementation record.</param>
        /// <returns><c>true</c> if they are considered equal, <c>false</c> otherwise.</returns>
        public bool Equals(MethodImplementation other)
        {
            return Equals(Declaration, other.Declaration) && Equals(Body, other.Body);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is MethodImplementation other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return ((Declaration != null ? Declaration.GetHashCode() : 0) * 397) ^ (Body != null ? Body.GetHashCode() : 0);
            }
        }
    }
}
