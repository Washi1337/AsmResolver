using System;
using System.Collections.Generic;
using System.Linq;

namespace AsmResolver.Net.Cts
{
    /// <summary>
    /// Provides methods for resolving metadata member references.
    /// </summary>
    public interface IMetadataResolver
    {
        /// <summary>
        /// Gets the assembly resolver used to resolve the assembly declaring the metadata reference.
        /// </summary>
        INetAssemblyResolver AssemblyResolver
        {
            get;
        }

        /// <summary>
        /// Resolves a reference to a type.
        /// </summary>
        /// <param name="type">The type reference to resolve.</param>
        /// <returns>The resolved type.</returns>
        TypeDefinition ResolveType(ITypeDescriptor type);

        /// <summary>
        /// Resolves a reference to a method.
        /// </summary>
        /// <param name="reference">The method reference to resolve.</param>
        /// <returns>The resolved method.</returns>
        MethodDefinition ResolveMethod(MemberReference reference);

        /// <summary>
        /// Resolves a reference to a field.
        /// </summary>
        /// <param name="reference">The field reference to resolve.</param>
        /// <returns>The resolved field.</returns>
        FieldDefinition ResolveField(MemberReference reference);
    }

    /// <summary>
    /// Provides a default mechanism for resolving member references.
    /// </summary>
    public class DefaultMetadataResolver : IMetadataResolver
    {
        private readonly SignatureComparer _signatureComparer = new SignatureComparer();

        public DefaultMetadataResolver(INetAssemblyResolver assemblyResolver)
        {
            if (assemblyResolver == null)
                throw new ArgumentNullException("assemblyResolver");
            AssemblyResolver = assemblyResolver;
            ThrowOnNotFound = true;
        }

        public INetAssemblyResolver AssemblyResolver
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the resolver should throw an exception when a member could not be resolved.
        /// </summary>
        public bool ThrowOnNotFound
        {
            get;
            set;
        }

        private object ThrowOrReturn(IFullNameProvider member)
        {
            if (ThrowOnNotFound)
                throw new MemberResolutionException(member);
            return null;
        }

        public TypeDefinition ResolveType(ITypeDescriptor type)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            type = type.GetElementType();

            var typeDef = type as TypeDefinition;
            if (typeDef != null)
                return typeDef;

            var assemblyDescriptor = type.ResolutionScope.GetAssembly();
            if (assemblyDescriptor == null)
                return (TypeDefinition)ThrowOrReturn(type);

            var assembly = AssemblyResolver.ResolveAssembly(assemblyDescriptor);
            if (assembly == null)
                return (TypeDefinition)ThrowOrReturn(type);

            IEnumerable<TypeDefinition> types;
            if (type.DeclaringTypeDescriptor != null)
            {
                var declaringType = ResolveType(type.DeclaringTypeDescriptor);
                if (declaringType == null)
                    return (TypeDefinition) ThrowOrReturn(type);
                types = declaringType.NestedClasses.Select(x => x.Class);
            }
            else
            {
                types = assembly.Modules[0].TopLevelTypes;
            }
            var definition = types.FirstOrDefault(x => _signatureComparer.Equals(x, type));
            return definition ?? (TypeDefinition)ThrowOrReturn(type);
        }

        public MethodDefinition ResolveMethod(MemberReference reference)
        {
            if (reference == null)
                throw new ArgumentNullException("reference");

            var typeDef = ResolveType(reference.Parent as ITypeDefOrRef);
            if (typeDef == null)
                return (MethodDefinition)ThrowOrReturn(reference);

            var method = typeDef.Methods.FirstOrDefault(x => _signatureComparer.Equals(x, reference));
            return method ?? (MethodDefinition)ThrowOrReturn(reference);
        }

        public FieldDefinition ResolveField(MemberReference reference)
        {
            if (reference == null)
                throw new ArgumentNullException("reference");

            var typeDef = ResolveType(reference.Parent as ITypeDefOrRef);
            if (typeDef == null)
                return (FieldDefinition)ThrowOrReturn(reference);

            var field = typeDef.Fields.FirstOrDefault(x => _signatureComparer.Equals(x, reference));
            return field ?? (FieldDefinition)ThrowOrReturn(reference);
        }
    }

    public class MemberResolutionException : Exception
    {
        public MemberResolutionException()
        {
        }

        public MemberResolutionException(string message)
            : base(message)
        {
        }

        public MemberResolutionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public MemberResolutionException(IFullNameProvider provider)
            : base(string.Format("The member {0} could not be resolved.", provider.FullName))
        {
        }
    }
}

