using System;

namespace AsmResolver.DotNet.Signatures
{
    /// <summary>
    /// Provides a context within a generic instantiation, including the type arguments of the enclosing type and method.
    /// </summary>
    public readonly struct GenericContext : IEquatable<GenericContext>
    {
        /// <summary>
        /// Creates a new instance of the <see cref="GenericContext"/> class.
        /// </summary>
        /// <param name="type">The type providing type arguments.</param>
        /// <param name="method">The method providing type arguments.</param>
        public GenericContext(IGenericArgumentsProvider? type, IGenericArgumentsProvider? method)
        {
            Type = type;
            Method = method;
        }

        /// <summary>
        /// Gets the object responsible for providing type arguments defined by the current generic type instantiation.
        /// </summary>
        public IGenericArgumentsProvider? Type
        {
            get;
        }

        /// <summary>
        /// Gets the object responsible for providing type arguments defined by the current generic method instantiation.
        /// </summary>
        public IGenericArgumentsProvider? Method
        {
            get;
        }

        /// <summary>
        /// Returns true if both Type and Method providers are null
        /// </summary>
        public bool IsEmpty => Type is null && Method is null;

        /// <summary>
        /// Enters a new generic context with a new type providing type arguments.
        /// </summary>
        /// <param name="type">The new type providing the type arguments.</param>
        /// <returns>The new generic context.</returns>
        public GenericContext WithType(IGenericArgumentsProvider type) => new GenericContext(type, Method);

        /// <summary>
        /// Enters a new generic context with a new method providing type arguments.
        /// </summary>
        /// <param name="method">The new method providing the type arguments.</param>
        /// <returns>The new generic context.</returns>
        public GenericContext WithMethod(IGenericArgumentsProvider method) => new GenericContext(Type, method);

        /// <summary>
        /// Resolves a type parameter to a type argument, based on the current generic context.
        /// </summary>
        /// <remarks>
        /// If a type parameter within the signature references a parameter that is not captured by the context
        /// (i.e. the corresponding generic argument provider is set to null),
        /// then this type parameter will not be substituted.
        /// </remarks>
        /// <param name="parameter">The parameter to get the argument value for.</param>
        /// <returns>The argument type.</returns>
        public TypeSignature GetTypeArgument(GenericParameterSignature parameter)
        {
            var argumentProvider = parameter.ParameterType switch
            {
                GenericParameterType.Type => Type,
                GenericParameterType.Method => Method,
                _ => throw new ArgumentOutOfRangeException()
            };

            if (argumentProvider is null)
                return parameter;

            if (parameter.Index >= 0 && parameter.Index < argumentProvider.TypeArguments.Count)
                return argumentProvider.TypeArguments[parameter.Index];

            throw new ArgumentOutOfRangeException();
        }


        /// <summary>
        /// Gets a type generic context from <see cref="TypeSpecification"/>.
        /// </summary>
        /// <param name="type">Type specification to get the generic context from.</param>
        /// <returns>Generic context.</returns>
        public static GenericContext FromType(TypeSpecification type) => type.Signature switch
        {
            GenericInstanceTypeSignature typeSig => new GenericContext(typeSig, null),
            _ => default
        };

        /// <summary>
        /// Gets a type generic context from <see cref="GenericInstanceTypeSignature"/>.
        /// </summary>
        /// <param name="type">Generic type signature to get the generic context from.</param>
        /// <returns>Generic context.</returns>
        public static GenericContext FromType(GenericInstanceTypeSignature type) => new(type, null);

        /// <summary>
        /// Gets a type generic context from <see cref="ITypeDescriptor"/>.
        /// </summary>
        /// <param name="type">Type to get the generic context from.</param>
        /// <returns>Generic context.</returns>
        public static GenericContext FromType(ITypeDescriptor type) => type switch
        {
            TypeSpecification typeSpecification => FromType(typeSpecification),
            GenericInstanceTypeSignature typeSig => FromType(typeSig),
            _ => default
        };

        /// <summary>
        /// Gets a method and/or type generic context from <see cref="MethodSpecification"/>.
        /// </summary>
        /// <param name="method">Method specification to get the generic context from.</param>
        /// <returns>Generic context.</returns>
        public static GenericContext FromMethod(MethodSpecification method)
        {
            return method.DeclaringType is TypeSpecification {Signature: GenericInstanceTypeSignature typeSig}
                ? new GenericContext(typeSig, method.Signature)
                : new GenericContext(null, method.Signature);
        }

        /// <summary>
        /// Gets a method and/or type generic context from <see cref="IMethodDescriptor"/>.
        /// </summary>
        /// <param name="method">Method to get the generic context from.</param>
        /// <returns>Generic context.</returns>
        public static GenericContext FromMethod(IMethodDescriptor method) =>
            method switch
            {
                MethodSpecification methodSpecification => FromMethod(methodSpecification),
                MemberReference member => FromMember(member),
                _ => default
            };

        /// <summary>
        /// Gets a type generic context from <see cref="IFieldDescriptor"/>.
        /// </summary>
        /// <param name="field">Field to get the generic context from.</param>
        /// <returns>Generic context.</returns>
        public static GenericContext FromField(IFieldDescriptor field) =>
            field switch
            {
                MemberReference member => FromMember(member),
                _ => default
            };

        /// <summary>
        /// Gets a type generic context from <see cref="MemberReference"/>.
        /// </summary>
        /// <param name="member">Member reference to get the generic context from.</param>
        /// <returns>Generic context.</returns>
        public static GenericContext FromMember(MemberReference member) =>
            member.DeclaringType switch
            {
                TypeSpecification type => FromType(type),
                _ => default
            };

        /// <summary>
        /// Gets a type generic context from <see cref="IMemberDescriptor"/>.
        /// </summary>
        /// <param name="member">Member to get the generic context from.</param>
        /// <returns>Generic context.</returns>
        public static GenericContext FromMember(IMemberDescriptor member) =>
            member switch
            {
                IMethodDescriptor method => FromMethod(method),
                IFieldDescriptor field => FromField(field),
                ITypeDescriptor type => FromType(type),
                _ => default
            };

        /// <summary>
        /// Determines whether two generic contexts have the same generic argument providers.
        /// </summary>
        /// <param name="other">The other context.</param>
        /// <returns><c>true</c> if the contexts are considered equal, <c>false</c> otherwise.</returns>
        public bool Equals(GenericContext other) => Equals(Type, other.Type) && Equals(Method, other.Method);

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is GenericContext other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return ((Type is { } type ? type.GetHashCode() : 0) * 397)
                       ^ (Method is { } method ? method.GetHashCode() : 0);
            }
        }
    }
}
