namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a type definition or reference that can be referenced by a TypeDefOrRef coded index.
    /// </summary>
    public interface ITypeDefOrRef : ITypeDescriptor, IMemberRefParent, IHasCustomAttribute
    {
        /// <summary>
        /// Gets the name of the type.
        /// </summary>
        new Utf8String? Name
        {
            get;
        }

        /// <summary>
        /// Gets the namespace the type resides in.
        /// </summary>
        new Utf8String? Namespace
        {
            get;
        }

        /// <summary>
        /// When this type is nested, gets the enclosing type.
        /// </summary>
        new ITypeDefOrRef? DeclaringType
        {
            get;
        }

        /// <summary>
        /// Imports the type using the provided reference importer object.
        /// </summary>
        /// <param name="importer">The reference importer to use for importing the type.</param>
        /// <returns>The imported type.</returns>
        new ITypeDefOrRef ImportWith(ReferenceImporter importer);
    }
}
