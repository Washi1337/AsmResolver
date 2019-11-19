namespace AsmResolver.DotNet
{
    internal static class Utilities
    {
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

        public static bool IsTypeOf(this ITypeDescriptor type, string ns, string name) =>
            type.Name == name && type.Namespace == ns;

    }
}