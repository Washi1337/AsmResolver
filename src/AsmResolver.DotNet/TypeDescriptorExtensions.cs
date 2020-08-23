using AsmResolver.DotNet.Signatures.Types;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Provides convenience extension methods to instances of <see cref="ITypeDescriptor"/>.
    /// </summary>
    public static class TypeDescriptorExtensions
    {
        /// <summary>
        /// Determines whether a type matches a namespace and name pair.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="ns">The namespace.</param>
        /// <param name="name">The name.</param>
        /// <returns><c>true</c> if the name and the namespace of the type matches the provided values,
        /// <c>false</c> otherwise.</returns>
        public static bool IsTypeOf(this ITypeDescriptor type, string ns, string name) =>
            type.Name == name && type.Namespace == ns;

        /// <summary>
        /// Constructs a new single-dimension, zero based array signature with the provided type descriptor
        /// as element type.
        /// </summary>
        /// <param name="type">The element type.</param>
        /// <returns>The constructed array type signature.</returns>
        public static SzArrayTypeSignature MakeSzArrayType(this ITypeDescriptor type) => 
            new SzArrayTypeSignature(type.ToTypeSignature());

        /// <summary>
        /// Constructs a new single-dimension, zero based array signature with the provided type descriptor
        /// as element type.
        /// </summary>
        /// <param name="type">The element type.</param>
        /// <param name="dimensionCount">The number of dimensions in the array.</param>
        /// <returns>The constructed array type signature.</returns>
        public static ArrayTypeSignature MakeArrayType(this ITypeDescriptor type, int dimensionCount) => 
            new ArrayTypeSignature(type.ToTypeSignature(), dimensionCount);

        /// <summary>
        /// Constructs a new single-dimension, zero based array signature with the provided type descriptor
        /// as element type.
        /// </summary>
        /// <param name="type">The element type.</param>
        /// <param name="dimensions">The dimensions of the array.</param>
        /// <returns>The constructed array type signature.</returns>
        public static ArrayTypeSignature MakeArrayType(this ITypeDescriptor type, params ArrayDimension[] dimensions) => 
            new ArrayTypeSignature(type.ToTypeSignature(), dimensions);
    }
}