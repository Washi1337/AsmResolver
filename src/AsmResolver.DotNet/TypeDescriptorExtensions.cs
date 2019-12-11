namespace AsmResolver.DotNet
{
    /// <summary>
    /// Provides convenience extension methods to instances of <see cref="ITypeDescriptor"/>.
    /// </summary>
    public static class TypeDescriptorExtensions
    {
        /// <summary>
        /// Computes the full name of a type descriptor, includings its namespace and/or declaring types.
        /// </summary>
        /// <param name="type">The type to obtain the full name for.</param>
        /// <returns>The full name.</returns>
        public static string GetFullName(this ITypeDescriptor type)
        {
            string prefix;
            if (type.DeclaringType != null)
                prefix = type.DeclaringType.FullName + "+";
            else if (type.Namespace != null)
                prefix = type.Namespace + ".";
            else
                prefix = null;

            return prefix + type.Name;
        }

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