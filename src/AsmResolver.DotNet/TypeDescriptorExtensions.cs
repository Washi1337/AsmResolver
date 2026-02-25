using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using AsmResolver.DotNet.Signatures;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Provides convenience extension methods to instances of <see cref="ITypeDescriptor"/>.
    /// </summary>
    public static class TypeDescriptorExtensions
    {
        /// <param name="type">The element type.</param>
        extension(ITypeDescriptor type)
        {
            /// <summary>
            /// Constructs a new generic instance type signature with the provided type descriptor as element type.
            /// as element type.
            /// </summary>
            /// <param name="context">The runtime context to assume when constructing the signature, if any.</param>
            /// <param name="typeArguments">The arguments to instantiate the type with.</param>
            /// <returns>The constructed by-reference type signature.</returns>
            public GenericInstanceTypeSignature MakeGenericInstanceType(RuntimeContext? context, params TypeSignature[] typeArguments)
            {
                return type.MakeGenericInstanceType(type.GetIsValueType(context), typeArguments);
            }

            /// <summary>
            /// Constructs a new generic instance type signature with the provided type descriptor as element type.
            /// as element type.
            /// </summary>
            /// <param name="isValueType"><c>true</c> if the type is a value type, <c>false</c> otherwise.</param>
            /// <param name="typeArguments">The arguments to instantiate the type with.</param>
            /// <returns>The constructed by-reference type signature.</returns>
            public GenericInstanceTypeSignature MakeGenericInstanceType(bool isValueType, params TypeSignature[] typeArguments)
            {
                return type.ToTypeDefOrRef().MakeGenericInstanceType(isValueType, typeArguments);
            }

            /// <summary>
            /// Determines whether a type matches a namespace and name pair.
            /// </summary>
            /// <param name="ns">The namespace.</param>
            /// <param name="name">The name.</param>
            /// <returns><c>true</c> if the name and the namespace of the type matches the provided values,
            /// <c>false</c> otherwise.</returns>
            public bool IsTypeOf(string? ns, string? name)
                => type.Name == name && type.Namespace == ns;

            /// <summary>
            /// Resolves the type descriptor to its definition given the provided runtime context.
            /// </summary>
            /// <param name="context">The context to assume when resolving the type.</param>
            /// <returns>The resolved type definition.</returns>
            /// <exception cref="InvalidOperationException">Occurs when the type could not be found.</exception>
            /// <exception cref="FileNotFoundException">Occurs when the declaring assembly could not be found.</exception>
            /// <exception cref="BadImageFormatException">Occurs when the declaring assembly is invalid.</exception>
            public TypeDefinition Resolve(RuntimeContext? context)
            {
                var status = type.Resolve(context, out var definition);
                return status == ResolutionStatus.Success ? definition! : ThrowStatusError(type, status);

                [DoesNotReturn]
                static TypeDefinition ThrowStatusError(ITypeDescriptor type, ResolutionStatus status) => status switch
                {
                    ResolutionStatus.MissingRuntimeContext => throw new ArgumentNullException(nameof(context), "The type reference requires a runtime context to be resolved"),
                    ResolutionStatus.InvalidReference => throw new InvalidOperationException($"The type reference is invalid."),
                    ResolutionStatus.AssemblyNotFound => throw new FileNotFoundException($"Could not find the file containing the declaring assembly {type.Scope?.GetAssembly().SafeToString()} of type {type}"),
                    ResolutionStatus.AssemblyBadImage => throw new BadImageFormatException($"The resolved declaring assembly for {type.Scope?.GetAssembly().SafeToString()} of type {type.SafeToString()} is in an incorrect format"),
                    ResolutionStatus.TypeNotFound => throw new InvalidOperationException($"The type {type.SafeToString()} does not exist in the resolved declaring assembly."),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

            /// <summary>
            /// Attempts to resolve the type descriptor in the provided context.
            /// </summary>
            /// <param name="context">The context to assume when resolving the type.</param>
            /// <param name="definition">The resolved type definition, or <c>null</c> if resolution failed.</param>
            /// <returns><c>true</c> if the resolution was successful, <c>false</c> otherwise.</returns>
            public bool TryResolve(RuntimeContext? context, [NotNullWhen(true)] out TypeDefinition? definition)
            {
                return type.Resolve(context, out definition) == ResolutionStatus.Success;
            }

            /// <summary>
            /// Determines whether a type is a value type or not.
            /// </summary>
            /// <param name="context">The runtime context that is assumed.</param>
            /// <returns><c>true</c> when the type is considered a value type, <c>false</c> when it is a reference type.</returns>
            /// <exception cref="ArgumentException">
            /// Occurs when it could not be determined whether the type descriptor is a value or reference type.
            /// </exception>
            public bool GetIsValueType(RuntimeContext? context)
            {
                return type.TryGetIsValueType(context)
                    ?? throw new ArgumentException($"Could not determine whether {type.SafeToString()} is a value type or not.");
            }
        }

        /// <param name="type">The element type.</param>
        extension(ITypeDefOrRef type)
        {
            /// <summary>
            /// Constructs a new generic instance type signature with the provided type descriptor as element type.
            /// as element type.
            /// </summary>
            /// <param name="isValueType"><c>true</c> if the type is a value type, <c>false</c> otherwise.</param>
            /// <param name="typeArguments">The arguments to instantiate the type with.</param>
            /// <returns>The constructed by-reference type signature.</returns>
            /// <remarks>
            /// This function can be used to avoid type resolution on type references.
            /// </remarks>
            public GenericInstanceTypeSignature MakeGenericInstanceType(bool isValueType, params TypeSignature[] typeArguments)
            {
                return new GenericInstanceTypeSignature(type, isValueType, typeArguments);
            }

            /// <summary>
            /// Transforms the type descriptor to an instance of a <see cref="TypeSignature"/>, which can be used in
            /// blob signatures.
            /// </summary>
            /// <param name="context">The runtime context to assume when constructing the signature, if any.</param>
            /// <returns>The constructed type signature instance.</returns>
            /// <remarks>
            /// This function can be used to avoid type resolution on type references.
            /// </remarks>
            public TypeSignature ToTypeSignature(RuntimeContext? context)
                => type.ToTypeSignature(type.GetIsValueType(context));

            /// <summary>
            /// Constructs a reference to a nested type.
            /// </summary>
            /// <param name="nestedTypeName">The name of the nested type.</param>
            /// <returns>The constructed reference.</returns>
            /// <exception cref="ArgumentOutOfRangeException">
            /// Occurs when the type cannot be used as a declaring type of the reference.
            /// </exception>
            public TypeReference CreateTypeReference(string nestedTypeName)
            {
                // Note: Runtime does not allow nesting with a TypeSpecification as parent.
                var parent = type switch
                {
                    TypeReference reference => reference,
                    TypeDefinition definition => definition.ToTypeReference(),
                    _ => throw new ArgumentOutOfRangeException()
                };

                return new TypeReference(type.ContextModule, parent, null, nestedTypeName);
            }

            /// <summary>
            /// Constructs a reference to a nested type.
            /// </summary>
            /// <param name="nestedTypeName">The name of the nested type.</param>
            /// <returns>The constructed reference.</returns>
            /// <exception cref="ArgumentOutOfRangeException">
            /// Occurs when the type cannot be used as a declaring type of the reference.
            /// </exception>
            public TypeReference CreateTypeReference(Utf8String nestedTypeName)
            {
                var parent = type switch
                {
                    TypeReference reference => reference,
                    TypeDefinition definition => definition.ToTypeReference(),
                    _ => throw new ArgumentOutOfRangeException()
                };

                return new TypeReference(type.ContextModule, parent, null, nestedTypeName);
            }

            /// <summary>
            /// Determines whether a type matches a namespace and name pair.
            /// </summary>
            /// <param name="ns">The namespace.</param>
            /// <param name="name">The name.</param>
            /// <returns><c>true</c> if the name and the namespace of the type matches the provided values,
            /// <c>false</c> otherwise.</returns>
            public bool IsTypeOfUtf8(Utf8String? ns, Utf8String? name)
                => type.Name == name && type.Namespace == ns;
        }

        /// <param name="scope">The scope the type is defined in.</param>
        extension(IResolutionScope scope)
        {
            /// <summary>
            /// Constructs a reference to a type within the provided resolution scope.
            /// </summary>
            /// <param name="ns">The namespace of the type.</param>
            /// <param name="name">The name of the type.</param>
            /// <returns>The constructed reference.</returns>
            public TypeReference CreateTypeReference(string? ns, string name)
            {
                return new TypeReference(scope.ContextModule, scope, ns, name);
            }

            /// <summary>
            /// Constructs a reference to a type within the provided resolution scope.
            /// </summary>
            /// <param name="ns">The namespace of the type.</param>
            /// <param name="name">The name of the type.</param>
            /// <returns>The constructed reference.</returns>
            public TypeReference CreateTypeReference(Utf8String? ns, Utf8String name)
            {
                return new TypeReference(scope.ContextModule, scope, ns, name);
            }
        }

        /// <param name="parent">The declaring member.</param>
        extension(IMemberRefParent parent)
        {
            /// <summary>
            /// Constructs a reference to a member declared within the provided parent member.
            /// </summary>
            /// <param name="memberName">The name of the member to reference.</param>
            /// <param name="signature">The signature of the member to reference.</param>
            /// <returns>The constructed reference.</returns>
            public MemberReference CreateMemberReference(Utf8String? memberName, MemberSignature? signature)
            {
                return new MemberReference(parent, memberName, signature);
            }

            /// <summary>
            /// Constructs a reference to a field declared within the provided parent member.
            /// </summary>
            /// <param name="fieldName">The name of the field to reference.</param>
            /// <param name="fieldType">The type of the field to reference.</param>
            /// <returns>The constructed reference.</returns>
            public IFieldDescriptor CreateFieldReference(Utf8String? fieldName, TypeSignature fieldType)
            {
                return new MemberReference(parent, fieldName, new FieldSignature(fieldType));
            }

            /// <summary>
            /// Constructs a reference to a field declared within the provided parent member.
            /// </summary>
            /// <param name="fieldName">The name of the field to reference.</param>
            /// <param name="signature">The signature of the field to reference.</param>
            /// <returns>The constructed reference.</returns>
            public IFieldDescriptor CreateFieldReference(Utf8String? fieldName, FieldSignature? signature)
            {
                return new MemberReference(parent, fieldName, signature);
            }

            /// <summary>
            /// Constructs a reference to a method declared within the provided parent member.
            /// </summary>
            /// <param name="methodName">The name of the method to reference.</param>
            /// <param name="signature">The signature of the method to reference.</param>
            /// <returns>The constructed reference.</returns>
            public IMethodDescriptor CreateMethodReference(Utf8String? methodName, MethodSignature? signature)
            {
                return new MemberReference(parent, methodName, signature);
            }
        }

        extension(IMethodDescriptor method)
        {
            /// <summary>
            /// Resolves the method descriptor to its definition given the provided runtime context.
            /// </summary>
            /// <param name="context">The context to assume when resolving the method.</param>
            /// <returns>The resolved method definition.</returns>
            /// <exception cref="InvalidOperationException">Occurs when the declaring type or the method within the declaring type could not be found.</exception>
            /// <exception cref="FileNotFoundException">Occurs when the declaring assembly could not be found.</exception>
            /// <exception cref="BadImageFormatException">Occurs when the declaring assembly is invalid.</exception>
            public MethodDefinition Resolve(RuntimeContext? context)
            {
                var status = method.Resolve(context, out var definition);
                return status == ResolutionStatus.Success ? definition! : ThrowStatusError(method, status);

                [DoesNotReturn]
                static MethodDefinition ThrowStatusError(IMethodDescriptor method, ResolutionStatus status) => status switch
                {
                    ResolutionStatus.MissingRuntimeContext => throw new ArgumentNullException(nameof(context), "The method reference requires a runtime context to be resolved"),
                    ResolutionStatus.InvalidReference => throw new InvalidOperationException($"The method reference is invalid."),
                    ResolutionStatus.AssemblyNotFound => throw new FileNotFoundException($"Could not find the file containing the declaring assembly {method.DeclaringType?.Scope?.GetAssembly().SafeToString()} of method {method.SafeToString()}."),
                    ResolutionStatus.AssemblyBadImage => throw new BadImageFormatException($"The resolved declaring assembly for {method.DeclaringType?.Scope?.GetAssembly().SafeToString()} of method {method.SafeToString()} is in an incorrect format."),
                    ResolutionStatus.TypeNotFound => throw new InvalidOperationException($"The declaring type of {method.SafeToString()} does not exist in the resolved declaring assembly."),
                    ResolutionStatus.MemberNotFound => throw new InvalidOperationException($"The method {method.SafeToString()} does not exist in the resolved declaring type."),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

            /// <summary>
            /// Attempts to resolve the method descriptor in the provided context.
            /// </summary>
            /// <param name="context">The context to assume when resolving the method.</param>
            /// <param name="definition">The resolved method definition, or <c>null</c> if resolution failed.</param>
            /// <returns><c>true</c> if the resolution was successful, <c>false</c> otherwise.</returns>
            public bool TryResolve(RuntimeContext? context, [NotNullWhen(true)] out MethodDefinition? definition)
            {
                return method.Resolve(context, out definition) == ResolutionStatus.Success;
            }
        }

        extension(IFieldDescriptor field)
        {
            /// <summary>
            /// Resolves the field descriptor to its definition given the provided runtime context.
            /// </summary>
            /// <param name="context">The context to assume when resolving the field.</param>
            /// <returns>The resolved field definition.</returns>
            /// <exception cref="InvalidOperationException">Occurs when the declaring type or the field within the declaring type could not be found.</exception>
            /// <exception cref="FileNotFoundException">Occurs when the declaring assembly could not be found.</exception>
            /// <exception cref="BadImageFormatException">Occurs when the declaring assembly is invalid.</exception>
            public FieldDefinition Resolve(RuntimeContext? context)
            {
                var status = field.Resolve(context, out var definition);
                return status == ResolutionStatus.Success ? definition! : ThrowStatusError(field, status);

                [DoesNotReturn]
                static FieldDefinition ThrowStatusError(IFieldDescriptor field, ResolutionStatus status) => status switch
                {
                    ResolutionStatus.MissingRuntimeContext => throw new ArgumentNullException(nameof(context), "The field reference requires a runtime context to be resolved"),
                    ResolutionStatus.InvalidReference => throw new InvalidOperationException($"The field reference is invalid."),
                    ResolutionStatus.AssemblyNotFound => throw new FileNotFoundException($"Could not find the file containing the declaring assembly {field.DeclaringType?.Scope?.GetAssembly().SafeToString()} of field {field.SafeToString()}."),
                    ResolutionStatus.AssemblyBadImage => throw new BadImageFormatException($"The resolved declaring assembly for {field.DeclaringType?.Scope?.GetAssembly().SafeToString()} of field {field.SafeToString()} is in an incorrect format."),
                    ResolutionStatus.TypeNotFound => throw new InvalidOperationException($"The declaring type of {field.SafeToString()} does not exist in the resolved declaring assembly."),
                    ResolutionStatus.MemberNotFound => throw new InvalidOperationException($"The field {field.SafeToString()} does not exist in the resolved declaring type."),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

            /// <summary>
            /// Attempts to resolve the field descriptor in the provided context.
            /// </summary>
            /// <param name="context">The context to assume when resolving the field.</param>
            /// <param name="definition">The resolved field definition, or <c>null</c> if resolution failed.</param>
            /// <returns><c>true</c> if the resolution was successful, <c>false</c> otherwise.</returns>
            public bool TryResolve(RuntimeContext? context, [NotNullWhen(true)] out FieldDefinition? definition)
            {
                return field.Resolve(context, out definition) == ResolutionStatus.Success;
            }
        }

    }
}
