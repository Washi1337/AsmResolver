using AsmResolver.DotNet.Blob;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Provides methods for constructing the full name of a member in a .NET module. 
    /// </summary>
    public static class FullNameGenerator
    {
        /// <summary>
        /// Computes the full name of a field definition, including its declaring type's full name, as well as its
        /// field type.
        /// </summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="declaringType">The declaring type of the field, if available.</param>
        /// <param name="signature">The signature of the field.</param>
        /// <returns>The full name</returns>
        public static string GetFieldFullName(string name, ITypeDescriptor declaringType, FieldSignature signature)
        {
            return declaringType == null
                ? $"{signature.FieldType} {name}"
                : $"{signature.FieldType} {declaringType}::{name}";
        }

        /// <summary>
        /// Computes the full name of a method definition, including its declaring type's full name, as well as its
        /// return type and parameters.
        /// </summary>
        /// <param name="name">The name of the method.</param>
        /// <param name="declaringType">The declaring type of the method if available.</param>
        /// <param name="signature">The signature of the method.</param>
        /// <returns>The full name</returns>
        public static string GetMethodFullName(string name, ITypeDescriptor declaringType, MethodSignature signature)
        {
            string parameterTypesString = GetParameterTypesString(signature);
            return declaringType is null
                ? $"{signature.ReturnType} {name}({parameterTypesString})"
                : $"{signature.ReturnType} {declaringType}::{name}({parameterTypesString})";
        }

        /// <summary>
        /// Computes the full name of a property definition, including its declaring type's full name, as well as its
        /// return type and parameters.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="declaringType">The declaring type of the property if available.</param>
        /// <param name="signature">The signature of the property.</param>
        /// <returns>The full name</returns>
        public static string GetPropertyFullName(string name, ITypeDescriptor declaringType, PropertySignature signature)
        {
            string parameterTypesString = signature.ParameterTypes.Count > 0
                ? $"[{GetParameterTypesString(signature)}]"
                : string.Empty;
            
            return declaringType is null
                ? $"{signature.ReturnType} {name}{parameterTypesString}"
                : $"{signature.ReturnType} {declaringType}::{name}{parameterTypesString}";
            
        }

        private static string GetParameterTypesString(MethodSignatureBase signature)
        {
            string parameterTypesString = string.Join(", ", signature.ParameterTypes)
                                          + (signature.IsSentinel ? ", ..." : string.Empty);
            return parameterTypesString;
        }

        /// <summary>
        /// Computes the full name of a type descriptor, including its namespace and/or declaring types.
        /// </summary>
        /// <param name="type">The type to obtain the full name for.</param>
        /// <returns>The full name.</returns>
        public static string GetTypeFullName(this ITypeDescriptor type)
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
    }
}