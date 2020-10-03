using System;
using AsmResolver.DotNet.Signatures.Types;

namespace AsmResolver.DotNet.Signatures
{
    /// <summary>
    /// Represents a boxed object in a custom attribute signature.
    /// </summary>
    public sealed class BoxedArgument
    {
        /// <summary>
        /// Creates a new instance of the <see cref="BoxedArgument"/> class.
        /// </summary>
        /// <param name="type">The value type of the boxed object.</param>
        /// <param name="value">The boxed value.</param>
        public BoxedArgument(TypeSignature type, object value)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Value = value;
        }
        
        /// <summary>
        /// Gets the type of the boxed argument.
        /// </summary>
        public TypeSignature Type
        {
            get;
        }

        /// <summary>
        /// Gets the boxed value.
        /// </summary>
        public object Value
        {
            get;
        }

        private bool Equals(BoxedArgument other)
        {
            return Equals(Type, other.Type) && Equals(Value, other.Value);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is BoxedArgument other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return ((Type != null ? Type.GetHashCode() : 0) * 397) ^ (Value != null ? Value.GetHashCode() : 0);
            }
        }

        /// <inheritdoc />
        public override string ToString() => $"({Type}) {(Value ?? "null")}";
    }
}