using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net
{
    public interface IMetadataResolver
    {
        INetAssemblyResolver AssemblyResolver
        {
            get;
        }

        TypeDefinition ResolveType(ITypeDescriptor type);

        MethodDefinition ResolveMethod(MemberReference reference);

        FieldDefinition ResolveField(MemberReference reference);
    }

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

            var typeDefTable = assembly.Header.GetStream<TableStream>().GetTable<TypeDefinition>();
            if (typeDefTable == null)
                return (TypeDefinition)ThrowOrReturn(type);

            var definition = typeDefTable.FirstOrDefault(x => _signatureComparer.MatchTypes(x, type));
            return definition ?? (TypeDefinition)ThrowOrReturn(type);
        }

        public MethodDefinition ResolveMethod(MemberReference reference)
        {
            if (reference == null)
                throw new ArgumentNullException("reference");

            var typeDef = ResolveType(reference.Parent as ITypeDefOrRef);
            if (typeDef == null)
                return (MethodDefinition)ThrowOrReturn(reference);

            var method = typeDef.Methods.FirstOrDefault(x => _signatureComparer.MatchMembers(x, reference));
            return method ?? (MethodDefinition)ThrowOrReturn(reference);
        }

        public FieldDefinition ResolveField(MemberReference reference)
        {
            if (reference == null)
                throw new ArgumentNullException("reference");

            var typeDef = ResolveType( reference.Parent as ITypeDefOrRef);
            if (typeDef == null)
                return (FieldDefinition)ThrowOrReturn(reference);

            var field = typeDef.Fields.FirstOrDefault(x => _signatureComparer.MatchMembers(x, reference));
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
