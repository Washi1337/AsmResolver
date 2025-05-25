using System;
using AsmResolver.DotNet.Signatures;

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
        public static bool IsTypeOf(this ITypeDescriptor type, string? ns, string? name) =>
            type.Name == name && type.Namespace == ns;

        /// <summary>
        /// Determines whether a type matches a namespace and name pair.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="ns">The namespace.</param>
        /// <param name="name">The name.</param>
        /// <returns><c>true</c> if the name and the namespace of the type matches the provided values,
        /// <c>false</c> otherwise.</returns>
        public static bool IsTypeOfUtf8(this ITypeDefOrRef type, Utf8String? ns, Utf8String? name) =>
            type.Name == name && type.Namespace == ns;

        /// <summary>
        /// Determines whether a type matches a namespace and name pair.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="ns">The namespace.</param>
        /// <param name="name">The name.</param>
        /// <returns><c>true</c> if the name and the namespace of the type matches the provided values,
        /// <c>false</c> otherwise.</returns>
        public static bool IsTypeOfUtf8(this ExportedType type, Utf8String? ns, Utf8String? name) =>
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
        /// Constructs a new boxed type signature with the provided type descriptor as element type.
        /// as element type.
        /// </summary>
        /// <param name="type">The element type.</param>
        /// <returns>The constructed boxed type signature.</returns>
        public static BoxedTypeSignature MakeBoxedType(this ITypeDescriptor type) =>
            new BoxedTypeSignature(type.ToTypeSignature());

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
            return type.MakeGenericInstanceType(type.IsValueType, typeArguments);
        }

        /// <summary>
        /// Constructs a new pointer type signature with the provided type descriptor as element type.
        /// as element type.
        /// </summary>
        /// <param name="type">The element type.</param>
        /// <param name="isValueType"><c>true</c> if the type is a value type, <c>false</c> otherwise.</param>
        /// <param name="typeArguments">The arguments to instantiate the type with.</param>
        /// <returns>The constructed by-reference type signature.</returns>
        /// <remarks>
        /// This function can be used to avoid type resolution on type references.
        /// </remarks>
        public static GenericInstanceTypeSignature MakeGenericInstanceType(
            this ITypeDescriptor type, bool isValueType, params TypeSignature[] typeArguments)
        {
            return new GenericInstanceTypeSignature(type.ToTypeDefOrRef(), isValueType, typeArguments);
        }

        /// <summary>
        /// Constructs a reference to a type within the provided resolution scope.
        /// </summary>
        /// <param name="scope">The scope the type is defined in.</param>
        /// <param name="ns">The namespace of the type.</param>
        /// <param name="name">The name of the type.</param>
        /// <returns>The constructed reference.</returns>
        public static TypeReference CreateTypeReference(this IResolutionScope scope, string? ns, string name)
        {
            return new TypeReference(scope, ns, name);
        }

        /// <summary>
        /// Constructs a reference to a type within the provided resolution scope.
        /// </summary>
        /// <param name="scope">The scope the type is defined in.</param>
        /// <param name="ns">The namespace of the type.</param>
        /// <param name="name">The name of the type.</param>
        /// <returns>The constructed reference.</returns>
        public static TypeReference CreateTypeReference(this IResolutionScope scope, Utf8String? ns, Utf8String name)
        {
            return new TypeReference(scope, ns, name);
        }

        /// <summary>
        /// Constructs a reference to a nested type.
        /// </summary>
        /// <param name="declaringType">The enclosing type.</param>
        /// <param name="nestedTypeName">The name of the nested type.</param>
        /// <returns>The constructed reference.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Occurs when <paramref name="declaringType"/> cannot be used as a declaring type of a type reference.
        /// </exception>
        public static TypeReference CreateTypeReference(this ITypeDefOrRef declaringType, string nestedTypeName)
        {
            var parent = declaringType switch
            {
                TypeReference reference => reference,
                TypeDefinition definition => definition.ToTypeReference(),
                _ => throw new ArgumentOutOfRangeException()
            };

            return new TypeReference(parent, null, nestedTypeName);
        }

        /// <summary>
        /// Constructs a reference to a nested type.
        /// </summary>
        /// <param name="declaringType">The enclosing type.</param>
        /// <param name="nestedTypeName">The name of the nested type.</param>
        /// <returns>The constructed reference.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Occurs when <paramref name="declaringType"/> cannot be used as a declaring type of a type reference.
        /// </exception>
        public static TypeReference CreateTypeReference(this ITypeDefOrRef declaringType, Utf8String nestedTypeName)
        {
            var parent = declaringType switch
            {
                TypeReference reference => reference,
                TypeDefinition definition => definition.ToTypeReference(),
                _ => throw new ArgumentOutOfRangeException()
            };

            return new TypeReference(parent, null, nestedTypeName);
        }

        /// <summary>
        /// Constructs a reference to a member declared within the provided parent member.
        /// </summary>
        /// <param name="parent">The declaring member.</param>
        /// <param name="memberName">The name of the member to reference.</param>
        /// <param name="signature">The signature of the member to reference.</param>
        /// <returns>The constructed reference.</returns>
        public static MemberReference CreateMemberReference(
            this IMemberRefParent parent,
            string? memberName,
            MemberSignature? signature)
        {
            return new MemberReference(parent, memberName, signature);
        }

        /// <summary>
        /// Constructs a reference to a member declared within the provided parent member.
        /// </summary>
        /// <param name="parent">The declaring member.</param>
        /// <param name="memberName">The name of the member to reference.</param>
        /// <param name="signature">The signature of the member to reference.</param>
        /// <returns>The constructed reference.</returns>
        public static MemberReference CreateMemberReference(
            this IMemberRefParent parent,
            Utf8String? memberName,
            MemberSignature? signature)
        {
            return new MemberReference(parent, memberName, signature);
        }
    }
}
