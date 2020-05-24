using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Collections;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Builder
{
    public partial class DotNetDirectoryBuffer : IMetadataTokenProvider
    {
        private readonly OneToOneRelation<TypeDefinition, MetadataToken> _typeDefTokens = new OneToOneRelation<TypeDefinition, MetadataToken>();
        private readonly OneToOneRelation<MethodDefinition, MetadataToken> _methodTokens = new OneToOneRelation<MethodDefinition, MetadataToken>();
        private readonly OneToOneRelation<FieldDefinition, MetadataToken> _fieldTokens = new OneToOneRelation<FieldDefinition, MetadataToken>();

        /// <inheritdoc />
        uint IMetadataTokenProvider.GetUserStringIndex(string value) => Metadata.UserStringsStream.GetStringIndex(value);

        /// <inheritdoc />
        public MetadataToken GetTypeReferenceToken(TypeReference type)
        {
            if (type == null)
                return 0;
            
            AssertIsImported(type);
            
            var table = Metadata.TablesStream.GetTable<TypeReferenceRow>(TableIndex.TypeRef);
            var row = new TypeReferenceRow(
                AddResolutionScope( type.Scope),
                Metadata.StringsStream.GetStringIndex(type.Name),
                Metadata.StringsStream.GetStringIndex(type.Namespace));

            var token = table.Add(row, type.MetadataToken.Rid);
            AddCustomAttributes(token, type);
            return token;
        }

        /// <inheritdoc />
        public MetadataToken GetTypeDefinitionToken(TypeDefinition type)
        {
            AssertIsImported(type);
            return _typeDefTokens.GetValue(type);
        }

        /// <inheritdoc />
        public MetadataToken GetFieldDefinitionToken(FieldDefinition field)
        {
            AssertIsImported(field);
            return _fieldTokens.GetValue(field);
        }

        /// <inheritdoc />
        public MetadataToken GetMethodDefinitionToken(MethodDefinition method)
        {
            AssertIsImported(method);
            return _methodTokens.GetValue(method);
        }

        /// <inheritdoc />
        public MetadataToken GetMemberReferenceToken(MemberReference member)
        {
            AssertIsImported(member);
            
            var table = Metadata.TablesStream.GetTable<MemberReferenceRow>(TableIndex.MemberRef);
            var row = new MemberReferenceRow(
                AddMemberRefParent(member.Parent),
                Metadata.StringsStream.GetStringIndex(member.Name),
                Metadata.BlobStream.GetBlobIndex(this, member.Signature));
            
            var token = table.Add(row, member.MetadataToken.Rid);
            AddCustomAttributes(token, member);
            return token;
        }

        /// <inheritdoc />
        public MetadataToken GetStandAloneSignatureToken(StandAloneSignature signature)
        {
            var table = Metadata.TablesStream.GetTable<StandAloneSignatureRow>(TableIndex.StandAloneSig);
            var row = new StandAloneSignatureRow(
                Metadata.BlobStream.GetBlobIndex(this, signature.Signature));
            
            var token = table.Add(row, signature.MetadataToken.Rid);
            AddCustomAttributes(token, signature);
            return token;
        }

        /// <inheritdoc />
        public MetadataToken GetAssemblyReferenceToken(AssemblyReference assembly)
        {
            AssertIsImported(assembly);
            
            var table = Metadata.TablesStream.GetTable<AssemblyReferenceRow>(TableIndex.AssemblyRef);

            var row = new AssemblyReferenceRow((ushort) assembly.Version.Major,
                (ushort) assembly.Version.Minor,
                (ushort) assembly.Version.Build,
                (ushort) assembly.Version.Revision,
                assembly.Attributes,
                Metadata.BlobStream.GetBlobIndex(assembly.PublicKeyOrToken),
                Metadata.StringsStream.GetStringIndex(assembly.Name),
                Metadata.StringsStream.GetStringIndex(assembly.Culture),
                Metadata.BlobStream.GetBlobIndex(assembly.HashValue));

            var token = table.Add(row, assembly.MetadataToken.Rid);
            AddCustomAttributes(token, assembly);
            return token;
        }

        /// <inheritdoc />
        public MetadataToken GetTypeSpecificationToken(TypeSpecification type)
        {
            AssertIsImported(type);
            
            var table = Metadata.TablesStream.GetTable<TypeSpecificationRow>(TableIndex.TypeSpec);
            var row = new TypeSpecificationRow(Metadata.BlobStream.GetBlobIndex(this, type.Signature));
            
            var token = table.Add(row, type.MetadataToken.Rid);
            AddCustomAttributes(token, type);
            return token;
        }

        /// <inheritdoc />
        public MetadataToken GetMethodSpecificationToken(MethodSpecification method)
        {
            var table = Metadata.TablesStream.GetTable<MethodSpecificationRow>(TableIndex.MethodSpec);
            var row = new MethodSpecificationRow(
                AddMethodDefOrRef(method.Method),
                Metadata.BlobStream.GetBlobIndex(this, method.Signature));
            
            var token = table.Add(row, method.MetadataToken.Rid);
            AddCustomAttributes(token, method);
            return token;
        }
    }
}