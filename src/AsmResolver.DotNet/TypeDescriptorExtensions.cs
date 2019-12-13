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

    }
}