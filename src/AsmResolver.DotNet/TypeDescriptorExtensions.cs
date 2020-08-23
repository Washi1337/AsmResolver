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

        /// <summary>
        /// Constructs a new by-reference type signature with the provided type descriptor as element type.
        /// as element type.
        /// </summary>
        /// <param name="type">The element type.</param>
        /// <returns>The constructed by-reference type signature.</returns>
        public static ByReferenceTypeSignature MakeByReferenceType(this ITypeDescriptor type) =>
            new ByReferenceTypeSignature(type.ToTypeSignature());

        /// <summary>
        /// Constructs a new pinned type signature with the provided type descriptor as element type.
        /// as element type.
        /// </summary>
        /// <param name="type">The element type.</param>
        /// <returns>The constructed by-reference type signature.</returns>
        public static PinnedTypeSignature MakePinnedType(this ITypeDescriptor type) =>
            new PinnedTypeSignature(type.ToTypeSignature());

        /// <summary>
        /// Constructs a new pointer type signature with the provided type descriptor as element type.
        /// as element type.
        /// </summary>
        /// <param name="type">The element type.</param>
        /// <returns>The constructed by-reference type signature.</returns>
        public static PointerTypeSignature MakePointerType(this ITypeDescriptor type) =>
            new PointerTypeSignature(type.ToTypeSignature());

        /// <summary>
        /// Constructs a new pointer type signature with the provided type descriptor as element type.
        /// as element type.
        /// </summary>
        /// <param name="type">The element type.</param>
        /// <param name="modifierType">The modifier type to add.</param>
        /// <param name="isRequired">Indicates whether the modifier is required or optional.</param>
        /// <returns>The constructed by-reference type signature.</returns>
        public static CustomModifierTypeSignature MakeModifierType(
            this ITypeDescriptor type, ITypeDefOrRef modifierType, bool isRequired)
        {
            return new CustomModifierTypeSignature(modifierType, isRequired, type.ToTypeSignature());
        }

        /// <summary>
        /// Constructs a new pointer type signature with the provided type descriptor as element type.
        /// as element type.
        /// </summary>
        /// <param name="type">The element type.</param>
        /// <param name="typeArguments">The arguments to instantiate the type with.</param>
        /// <returns>The constructed by-reference type signature.</returns>
        public static GenericInstanceTypeSignature MakeGenericInstanceType(
            this ITypeDescriptor type, params TypeSignature[] typeArguments)
        {
            return new GenericInstanceTypeSignature(type.ToTypeDefOrRef(), type.IsValueType, typeArguments);
        }
    }
}