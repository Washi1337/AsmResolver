using AsmResolver.PE.DotNet;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Builder
{
    public partial class DotNetDirectoryBuffer
    {
        public DotNetDirectoryBuffer(ModuleDefinition module, IMethodBodySerializer methodBodySerializer, IMetadataBuffer metadata)
        {
            Module = module;
            MethodBodySerializer = methodBodySerializer;
            Metadata = metadata;
        }
        
        public ModuleDefinition Module
        {
            get;
        }

        public IMethodBodySerializer MethodBodySerializer
        {
            get;
        }

        public IMetadataBuffer Metadata
        {
            get;
        }

        private void AssertIsImported(IModuleProvider member)
        {
            if (member.Module != Module)
                throw new MemberNotImportedException((IMemberDescriptor) member);
        }

        public IDotNetDirectory CreateDirectory()
        {
            var directory = new DotNetDirectory();
            directory.Metadata = Metadata.CreateMetadata();
            return directory;
        }

        private MetadataToken AddAssemblyReference(AssemblyReference assembly)
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

        public MetadataToken AddTypeReference(TypeReference type)
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

        public MetadataToken AddMemberReference(MemberReference member)
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

        public MetadataToken AddTypeSpecification(TypeSpecification type)
        {
            AssertIsImported(type);
            
            var table = Metadata.TablesStream.GetTable<TypeSpecificationRow>(TableIndex.TypeSpec);
            var row = new TypeSpecificationRow(Metadata.BlobStream.GetBlobIndex(this, type.Signature));
            
            var token = table.Add(row, type.MetadataToken.Rid);
            AddCustomAttributes(token, type);
            return token;
        }

        public MetadataToken AddStandAloneSignature(StandAloneSignature signature)
        {
            var table = Metadata.TablesStream.GetTable<StandAloneSignatureRow>(TableIndex.StandAloneSig);
            var row = new StandAloneSignatureRow(
                Metadata.BlobStream.GetBlobIndex(this, signature.Signature));
            
            var token = table.Add(row, signature.MetadataToken.Rid);
            AddCustomAttributes(token, signature);
            return token;
        }

        public MetadataToken AddMethodSpecification(MethodSpecification method)
        {
            var table = Metadata.TablesStream.GetTable<MethodSpecificationRow>(TableIndex.MethodSpec);
            var row = new MethodSpecificationRow(
                AddMethodDefOrRef(method.Method),
                Metadata.BlobStream.GetBlobIndex(this, method.Signature));
            
            var token = table.Add(row, method.MetadataToken.Rid);
            AddCustomAttributes(token, method);
            return token;
        }

        private void AddCustomAttributes(MetadataToken ownerToken, IHasCustomAttribute provider)
        {
            foreach (var attribute in provider.CustomAttributes)
                AddCustomAttribute(ownerToken, attribute);
        }

        private void AddCustomAttribute(MetadataToken ownerToken, CustomAttribute attribute)
        {
            var table = Metadata.TablesStream.GetTable<CustomAttributeRow>(TableIndex.CustomAttribute);

            var encoder = Metadata.TablesStream.GetIndexEncoder(CodedIndex.HasCustomAttribute);
            var row = new CustomAttributeRow(
                encoder.EncodeToken(ownerToken),
                AddCustomAttributeType(attribute.Constructor),
                Metadata.BlobStream.GetBlobIndex(this, attribute.Signature));

            table.Add(row, attribute.MetadataToken.Rid);
        }
    }
}