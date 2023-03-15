using System.Collections.Generic;

namespace AsmResolver.DotNet.Signatures.Types
{
    /// <summary>
    /// Represents a type signature representing an array.
    /// </summary>
    public abstract class ArrayBaseTypeSignature : TypeSpecificationSignature
    {
        /// <summary>
        /// Initializes an array type signature.
        /// </summary>
        /// <param name="baseType">The element type of the array.</param>
        protected ArrayBaseTypeSignature(TypeSignature baseType)
            : base(baseType)
        {
        }

        /// <inheritdoc />
        public override bool IsValueType => false;

        /// <summary>
        /// Gets the number of dimensions this array defines.
        /// </summary>
        public abstract int Rank
        {
            get;
        }

        /// <summary>
        /// Obtains the dimensions this array defines.
        /// </summary>
        /// <returns>The dimensions.</returns>
        public abstract IEnumerable<ArrayDimension> GetDimensions();

        /// <inheritdoc />
        protected override bool IsDirectlyCompatibleWith(TypeSignature other)
        {
            if (base.IsDirectlyCompatibleWith(other))
                return true;

            TypeSignature? elementType = null;
            if (other is ArrayBaseTypeSignature otherArrayType && Rank == otherArrayType.Rank)
            {
                // Arrays are only compatible if they have the same rank.
                elementType = otherArrayType.BaseType;
            }
            else if (Rank == 1
                     && other is GenericInstanceTypeSignature genericInstanceType
                     && genericInstanceType.GenericType.IsTypeOf("System.Collections.Generic", "IList`1"))
            {
                // Arrays are also compatible with IList<T> if they've only one dimension.
                elementType = genericInstanceType.TypeArguments[0];
            }

            if (elementType is null)
                return false;

            var v = BaseType.GetUnderlyingType();
            var w = elementType.GetUnderlyingType();

            return SignatureComparer.Default.Equals(v.GetReducedType(), w.GetReducedType())
                   || v.IsCompatibleWith(w);
        }
    }
}
