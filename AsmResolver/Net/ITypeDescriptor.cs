using AsmResolver.Net.Cts;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net
{
    public interface ITypeDescriptor : IFullNameProvider
    {
        /// <summary>
        /// Gets the namespace of the type.
        /// </summary>
        string Namespace
        {
            get;
        }

        /// <summary>
        /// When nested, gets the descriptor of the enclosing type that declares the type.
        /// </summary>
        ITypeDescriptor DeclaringTypeDescriptor
        {
            get;
        }

        /// <summary>
        /// Gets the resolution scope of the type.
        /// </summary>
        IResolutionScope ResolutionScope
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether the type is a value type or not. 
        /// </summary>
        bool IsValueType
        {
            get;
        }

        /// <summary>
        /// Traverses the type descriptor and determines the element type this descriptor was based on.
        /// If the descriptor is an element type on its own, it will return the current instance.
        /// If the descriptor is a type signature, it will return the type that this signature was based on. 
        /// </summary>
        /// <returns>The element type.</returns>
        ITypeDescriptor GetElementType();

        /// <summary>
        /// Gets or creates a new type signature based on this type.
        /// </summary>
        /// <returns>The signature.</returns>
        TypeSignature ToTypeSignature();

        /// <summary>
        /// Gets or creates a new type reference based on this type.
        /// </summary>
        /// <returns>The reference.</returns>
        ITypeDefOrRef ToTypeDefOrRef();
    }

}