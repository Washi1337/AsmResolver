using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;

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
        public static string GetFieldFullName(string? name, ITypeDescriptor? declaringType, FieldSignature? signature)
        {
            string fieldTypeString = signature?.FieldType.FullName ?? TypeSignature.NullTypeToString;

            return declaringType is null
                ? $"{fieldTypeString} {name}"
                : $"{fieldTypeString} {declaringType}::{name}";
        }

        /// <summary>
        /// Computes the full name of a method definition, including its declaring type's full name, as well as its
        /// return type and parameters.
        /// </summary>
        /// <param name="name">The name of the method.</param>
        /// <param name="declaringType">The declaring type of the method if available.</param>
        /// <param name="signature">The signature of the method.</param>
        /// <returns>The full name</returns>
        public static string GetMethodFullName(string? name, ITypeDescriptor? declaringType, MethodSignature? signature)
        {
            if (signature?.GenericParameterCount > 0)
            {
                return GetMethodFullName(name, declaringType, signature,
                    Enumerable.Repeat<string>("?", signature.GenericParameterCount));
            }

            string returnTypeString = signature?.ReturnType.FullName ?? TypeSignature.NullTypeToString;
            string parameterTypesString = GetParameterTypesString(signature);

            return declaringType is null
                ? $"{returnTypeString} {name}({parameterTypesString})"
                : $"{returnTypeString} {declaringType}::{name}({parameterTypesString})";
        }

        /// <summary>
        /// Computes the full name of a method specification, including its declaring type's full name, as well as its
        /// return type, parameters and any type arguments.
        /// </summary>
        /// <param name="name">The name of the method.</param>
        /// <param name="declaringType">The declaring type of the method if available.</param>
        /// <param name="signature">The signature of the method.</param>
        /// <param name="typeArguments">The type arguments.</param>
        /// <returns>The full name</returns>
        public static string GetMethodFullName(
            string? name,
            ITypeDescriptor? declaringType,
            MethodSignature? signature,
            IEnumerable<string> typeArguments)
        {
            string returnTypeString = signature?.ReturnType.FullName ?? TypeSignature.NullTypeToString;
            string parameterTypesString = GetParameterTypesString(signature);

            string[] argumentNames = typeArguments.ToArray();
            string typeArgumentsString = argumentNames.Length>0
                ? $"<{string.Join(", ", argumentNames)}>"
                : string.Empty;

            return declaringType is null
                ? $"{returnTypeString} {name}{typeArgumentsString}({parameterTypesString})"
                : $"{returnTypeString} {declaringType}::{name}{typeArgumentsString}({parameterTypesString})";
        }

        /// <summary>
        /// Computes the full name of a property definition, including its declaring type's full name, as well as its
        /// return type and parameters.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="declaringType">The declaring type of the property if available.</param>
        /// <param name="signature">The signature of the property.</param>
        /// <returns>The full name</returns>
        public static string GetPropertyFullName(string? name, ITypeDescriptor? declaringType, PropertySignature? signature)
        {
            string propertyTypeString = signature?.ReturnType.FullName ?? TypeSignature.NullTypeToString;
            string parameterTypesString = signature?.ParameterTypes.Count > 0
                ? $"[{GetParameterTypesString(signature)}]"
                : string.Empty;

            return declaringType is null
                ? $"{propertyTypeString} {name}{parameterTypesString}"
                : $"{propertyTypeString} {declaringType}::{name}{parameterTypesString}";
        }

        /// <summary>
        /// Computes the full name of a event definition, including its declaring type's full name, as well as its
        /// event type.
        /// </summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="declaringType">The declaring type of the field, if available.</param>
        /// <param name="eventType">The type of the event.</param>
        /// <returns>The full name</returns>
        public static string GetEventFullName(string? name, ITypeDescriptor? declaringType, ITypeDefOrRef? eventType)
        {
            return declaringType is null
                ? $"{eventType} {name}"
                : $"{eventType} {declaringType}::{name}";
        }

        private static string GetParameterTypesString(MethodSignatureBase? signature)
        {
            if (signature is null)
                return string.Empty;

            string parametersString = string.Join(", ", signature.ParameterTypes);
            string sentinelSuffix = signature.IsSentinel
                ? ", ..."
                : string.Empty;

            return $"{parametersString}{sentinelSuffix}";
        }

        /// <summary>
        /// Computes the full name of a type descriptor, including its namespace and/or declaring types.
        /// </summary>
        /// <param name="type">The type to obtain the full name for.</param>
        /// <returns>The full name.</returns>
        public static string GetTypeFullName(this ITypeDescriptor type)
        {
            string name = type.Name ?? MetadataMember.NullName;

            if (type.DeclaringType is not null)
                return $"{type.DeclaringType.FullName}+{name}";

            return !string.IsNullOrEmpty(type.Namespace)
                ? $"{type.Namespace}.{name}"
                : name;
        }

    }
}
