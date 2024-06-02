using System;
using System.Collections.Generic;
using System.Text;
using AsmResolver.Collections;
using AsmResolver.DotNet.Signatures;
using AsmResolver.Shims;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Provides methods for constructing the full name of a member in a .NET module.
    /// </summary>
    public sealed class MemberNameGenerator : ITypeSignatureVisitor<StringBuilder, StringBuilder>
    {
        [ThreadStatic]
        private static StringBuilder? _builder;

        private MemberNameGenerator()
        {
        }

        /// <summary>
        /// Gets the singleton instance for the member name generator.
        /// </summary>
        private static MemberNameGenerator Instance
        {
            get;
        } = new();

        private static StringBuilder GetBuilder()
        {
            _builder ??= new StringBuilder();
            _builder.Clear();
            return _builder;
        }

        /// <summary>
        /// Computes the full name of a type descriptor, including its namespace and/or declaring types.
        /// </summary>
        /// <param name="type">The type to obtain the full name for.</param>
        /// <returns>The full name.</returns>
        public static string GetTypeFullName(ITypeDescriptor type)
        {
            var state = GetBuilder();
            return AppendTypeFullName(state, type).ToString();
        }

        /// <summary>
        /// Computes the full name of a field definition, including its declaring type's full name, as well as its
        /// field type.
        /// </summary>
        /// <param name="descriptor">The field</param>
        /// <returns>The full name</returns>
        public static string GetFieldFullName(IFieldDescriptor descriptor)
        {
            var state = GetBuilder();

            AppendTypeFullName(state, descriptor.Signature?.FieldType);
            state.Append(' ');
            AppendMemberDeclaringType(state, descriptor.DeclaringType);

            return state.Append(descriptor.Name ?? MetadataMember.NullName).ToString();
        }

        /// <summary>
        /// Computes the full name of a method reference, including its declaring type's full name, as well as its
        /// return type and parameters.
        /// </summary>
        /// <param name="reference">The reference</param>
        /// <returns>The full name</returns>
        public static string GetMethodFullName(MemberReference reference)
        {
            var state = GetBuilder();

            var signature = reference.Signature as MethodSignature;

            AppendTypeFullName(state, signature?.ReturnType);
            state.Append(' ');
            AppendMemberDeclaringType(state, reference.DeclaringType);
            state.Append(reference.Name ?? MetadataMember.NullName);

            AppendTypeArgumentPlaceholders(state, signature);

            state.Append('(');
            AppendSignatureParameterTypes(state, signature);
            state.Append(')');

            return state.ToString();
        }

        /// <summary>
        /// Computes the full name of a method definition, including its declaring type's full name, as well as its
        /// return type, parameters and any type arguments.
        /// </summary>
        /// <param name="definition">The definition</param>
        /// <returns>The full name</returns>
        public static string GetMethodFullName(MethodDefinition definition)
        {
            var state = GetBuilder();

            var signature = definition.Signature;

            AppendTypeFullName(state, signature?.ReturnType);
            state.Append(' ');
            AppendMemberDeclaringType(state, definition.DeclaringType);
            state.Append(definition.Name ?? MetadataMember.NullName);

            AppendTypeParameters(state, definition.GenericParameters);

            state.Append('(');
            AppendSignatureParameterTypes(state, signature);
            state.Append(')');

            return state.ToString();
        }

        /// <summary>
        /// Computes the full name of a method specification, including its declaring type's full name, as well as its
        /// return type, parameters and any type arguments.
        /// </summary>
        /// <param name="specification">The specification</param>
        /// <returns>The full name</returns>
        public static string GetMethodFullName(MethodSpecification specification)
        {
            var state = GetBuilder();

            var signature = specification.Method?.Signature;

            AppendTypeFullName(state, signature?.ReturnType);
            state.Append(' ');
            AppendMemberDeclaringType(state, specification.DeclaringType);
            state.Append(specification.Name);

            AppendTypeParameters(state, specification.Signature?.TypeArguments ?? ArrayShim.Empty<TypeSignature>());

            state.Append('(');
            AppendSignatureParameterTypes(state, signature);
            state.Append(')');

            return state.ToString();
        }

        /// <summary>
        /// Computes the full name of a property definition, including its declaring type's full name, as well as its
        /// return type and parameters.
        /// </summary>
        /// <param name="definition">The property</param>
        /// <returns>The full name</returns>
        public static string GetPropertyFullName(PropertyDefinition definition)
        {
            var state = GetBuilder();

            var signature = definition.Signature;

            AppendTypeFullName(state, signature?.ReturnType);
            state.Append(' ');
            AppendMemberDeclaringType(state, definition.DeclaringType);
            state.Append(definition.Name ?? MetadataMember.NullName);

            if (signature?.ParameterTypes.Count > 0)
            {
                state.Append('[');
                AppendSignatureParameterTypes(state, signature);
                state.Append(']');
            }

            return state.ToString();
        }

        /// <summary>
        /// Computes the full name of a event definition, including its declaring type's full name, as well as its
        /// event type.
        /// </summary>
        /// <param name="definition">The event</param>
        /// <returns>The full name</returns>
        public static string GetEventFullName(EventDefinition definition)
        {
            var state = GetBuilder();

            AppendTypeFullName(state, definition.EventType);
            state.Append(' ');
            AppendMemberDeclaringType(state, definition.DeclaringType);
            state.Append(definition.Name);

            return state.ToString();
        }

        private static StringBuilder AppendMemberDeclaringType(StringBuilder state, ITypeDescriptor? declaringType)
        {
            if (declaringType is not null)
            {
                AppendTypeFullName(state, declaringType);
                state.Append("::");
            }

            return state;
        }

        private static StringBuilder AppendSignatureParameterTypes(StringBuilder state, MethodSignatureBase? signature)
        {
            if (signature is null)
                return state;

            for (int i = 0; i < signature.ParameterTypes.Count; i++)
            {
                signature.ParameterTypes[i].AcceptVisitor(Instance, state);
                if (i < signature.ParameterTypes.Count - 1)
                    state.Append(", ");
            }

            if (signature.CallingConvention == CallingConventionAttributes.VarArg)
                state.Append("...");

            return state;
        }

        private static StringBuilder AppendTypeParameters(StringBuilder state, IList<GenericParameter> typeArguments)
        {
            if (typeArguments.Count > 0)
            {
                state.Append('<');
                AppendCommaSeparatedCollection(state, typeArguments, static (s, t) => s.Append(t.Name));
                state.Append('>');
            }

            return state;
        }

        private static StringBuilder AppendTypeParameters(StringBuilder state, IList<TypeSignature> typeArguments)
        {
            if (typeArguments.Count > 0)
            {
                state.Append('<');
                AppendCommaSeparatedCollection(state, typeArguments, static (s, t) => t.AcceptVisitor(Instance, s));
                state.Append('>');
            }

            return state;
        }

        private static StringBuilder AppendTypeFullName(StringBuilder state, ITypeDescriptor? type)
        {
            switch (type)
            {
                case TypeSignature signature:
                    return signature.AcceptVisitor(Instance, state);

                case ITypeDefOrRef reference:
                    return AppendTypeFullName(state, reference);

                case null:
                    return state.Append(TypeSignature.NullTypeToString);

                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }
        }

        private static StringBuilder AppendTypeFullName(StringBuilder state, ITypeDefOrRef? type)
        {
            if (type is null)
                return state.Append(TypeSignature.NullTypeToString);

            if (type is TypeSpecification specification)
                return AppendTypeFullName(state, specification.Signature);

            if (type.DeclaringType is { } declaringType)
            {
                AppendTypeFullName(state, declaringType);
                state.Append('+');
            }
            else if (!string.IsNullOrEmpty(type.Namespace))
            {
                state.Append(type.Namespace);
                state.Append('.');
            }

            return state.Append(type.Name ?? MetadataMember.NullName);
        }

        private static StringBuilder AppendTypeArgumentPlaceholders(StringBuilder state, MethodSignature? signature)
        {
            if (signature?.GenericParameterCount > 0)
            {
                state.Append('<');

                for (int i = 0; i < signature.GenericParameterCount; i++)
                {
                    state.Append('?');
                    if (i < signature.GenericParameterCount - 1)
                        state.Append(", ");
                }

                state.Append('>');
            }

            return state;
        }

        private static StringBuilder AppendMethodSignature(StringBuilder state, MethodSignature signature)
        {
            if (signature.HasThis)
                state.Append("instance ");

            signature.ReturnType.AcceptVisitor(Instance, state);

            state.Append(" *");

            AppendTypeArgumentPlaceholders(state, signature);

            state.Append('(');
            AppendSignatureParameterTypes(state, signature);
            return state.Append(')');
        }

        private static StringBuilder AppendCommaSeparatedCollection<T>(
            StringBuilder state,
            IList<T> collection,
            Action<StringBuilder, T> action)
        {
            for (int i = 0; i < collection.Count; i++)
            {
                action(state, collection[i]);
                if (i < collection.Count - 1)
                    state.Append(", ");
            }

            return state;
        }

        /// <inheritdoc />
        StringBuilder ITypeSignatureVisitor<StringBuilder, StringBuilder>.VisitArrayType(ArrayTypeSignature signature, StringBuilder state)
        {
            signature.BaseType.AcceptVisitor(this, state);

            state.Append('[');

            AppendCommaSeparatedCollection(state, signature.Dimensions, static (s, d) =>
            {
                if (d.LowerBound.HasValue)
                {
                    if (d.Size.HasValue)
                    {
                        AppendDimensionBound(s, d.LowerBound.Value, d.Size.Value);
                    }
                    else
                    {
                        s.Append(d.LowerBound.Value)
                            .Append("...");
                    }
                }

                if (d.Size.HasValue)
                    AppendDimensionBound(s, 0, d.Size.Value);

                static void AppendDimensionBound(StringBuilder state, int low, int size)
                {
                    state.Append(low)
                        .Append("...")
                        .Append(low + size - 1);
                }
            });

            return state.Append(']');
        }

        /// <inheritdoc />
        StringBuilder ITypeSignatureVisitor<StringBuilder, StringBuilder>.VisitBoxedType(BoxedTypeSignature signature, StringBuilder state)
        {
            return signature.BaseType.AcceptVisitor(this, state);
        }

        /// <inheritdoc />
        StringBuilder ITypeSignatureVisitor<StringBuilder, StringBuilder>.VisitByReferenceType(ByReferenceTypeSignature signature, StringBuilder state)
        {
            return signature.BaseType
                .AcceptVisitor(this, state)
                .Append('&');
        }

        /// <inheritdoc />
        StringBuilder ITypeSignatureVisitor<StringBuilder, StringBuilder>.VisitCorLibType(CorLibTypeSignature signature, StringBuilder state)
        {
            return state.Append("System.").Append(signature.Name);
        }

        /// <inheritdoc />
        StringBuilder ITypeSignatureVisitor<StringBuilder, StringBuilder>.VisitCustomModifierType(CustomModifierTypeSignature signature, StringBuilder state)
        {
            signature.BaseType.AcceptVisitor(this, state);
            state.Append(signature.IsRequired ? " modreq(" : " modopt(");
            AppendTypeFullName(state, signature.ModifierType);
            return state.Append(')');
        }

        /// <inheritdoc />
        StringBuilder ITypeSignatureVisitor<StringBuilder, StringBuilder>.VisitGenericInstanceType(GenericInstanceTypeSignature signature, StringBuilder state)
        {
            AppendTypeFullName(state, signature.GenericType);
            return AppendTypeParameters(state, signature.TypeArguments);
        }

        /// <inheritdoc />
        StringBuilder ITypeSignatureVisitor<StringBuilder, StringBuilder>.VisitGenericParameter(GenericParameterSignature signature, StringBuilder state)
        {
            state.Append(signature.ParameterType switch
            {
                GenericParameterType.Type => "!",
                GenericParameterType.Method => "!!",
                _ => throw new ArgumentOutOfRangeException()
            });

            return state.Append(signature.Index);
        }

        /// <inheritdoc />
        StringBuilder ITypeSignatureVisitor<StringBuilder, StringBuilder>.VisitPinnedType(PinnedTypeSignature signature, StringBuilder state)
        {
            return signature.BaseType.AcceptVisitor(this, state);
        }

        /// <inheritdoc />
        StringBuilder ITypeSignatureVisitor<StringBuilder, StringBuilder>.VisitPointerType(PointerTypeSignature signature, StringBuilder state)
        {
            return signature.BaseType
                .AcceptVisitor(this, state)
                .Append('*');
        }

        /// <inheritdoc />
        StringBuilder ITypeSignatureVisitor<StringBuilder, StringBuilder>.VisitSentinelType(SentinelTypeSignature signature, StringBuilder state)
        {
            return state.Append("<<<SENTINEL>>>");
        }

        /// <inheritdoc />
        StringBuilder ITypeSignatureVisitor<StringBuilder, StringBuilder>.VisitSzArrayType(SzArrayTypeSignature signature, StringBuilder state)
        {
            return signature.BaseType
                .AcceptVisitor(this, state)
                .Append("[]");
        }

        /// <inheritdoc />
        StringBuilder ITypeSignatureVisitor<StringBuilder, StringBuilder>.VisitTypeDefOrRef(TypeDefOrRefSignature signature, StringBuilder state)
        {
            return AppendTypeFullName(state, signature.Type);
        }

        /// <inheritdoc />
        StringBuilder ITypeSignatureVisitor<StringBuilder, StringBuilder>.VisitFunctionPointerType(FunctionPointerTypeSignature signature, StringBuilder state)
        {
            state.Append("method ");
            return AppendMethodSignature(state, signature.Signature);
        }
    }
}
