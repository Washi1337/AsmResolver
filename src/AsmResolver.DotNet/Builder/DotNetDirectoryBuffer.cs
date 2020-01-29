using System;
using System.Collections.Generic;
using AsmResolver.DotNet.Builder.Blob;
using AsmResolver.DotNet.Builder.Guid;
using AsmResolver.DotNet.Builder.Strings;
using AsmResolver.DotNet.Builder.Tables;
using AsmResolver.DotNet.Collections;
using AsmResolver.PE.DotNet;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using AsmResolver.PE.DotNet.Metadata.UserStrings;

namespace AsmResolver.DotNet.Builder
{
    public partial class DotNetDirectoryBuffer : ITokenProvider
    {
        private readonly OneToOneRelation<TypeDefinition, MetadataToken> _typeDefTokens = new OneToOneRelation<TypeDefinition, MetadataToken>();

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

        private void AddModule(ModuleDefinition module)
        {
            var stringsStream = Metadata.StringsStream;
            var guidStream = Metadata.GuidStream;

            var table = Metadata.TablesStream.GetTable<ModuleDefinitionRow>(TableIndex.Module);
            table.Add(new ModuleDefinitionRow(
                module.Generation,
                stringsStream.GetStringIndex(module.Name),
                guidStream.GetGuidIndex(module.Mvid),
                guidStream.GetGuidIndex(module.EncId),
                guidStream.GetGuidIndex(module.EncBaseId)));

            AddTypeDefinitionsInModule(module);
        }

        private void AddTypeDefinitionsInModule(ModuleDefinition module)
        {
            AddTypeDefinitionStubs(module);
            AddTypeDefinitionMembers();
        }

        private void AddTypeDefinitionStubs(ModuleDefinition module)
        {
            foreach (var type in module.GetAllTypes())
                AddTypeDefinitionStub(type);
        }

        private MetadataToken AddTypeDefinitionStub(TypeDefinition type)
        {
            var table = Metadata.TablesStream.GetTable<TypeDefinitionRow>(TableIndex.TypeDef);

            var row = new TypeDefinitionRow(
                type.Attributes,
                Metadata.StringsStream.GetStringIndex(type.Name),
                Metadata.StringsStream.GetStringIndex(type.Namespace),
                0,
                0,
                0);

            var token = table.Add(row);
            _typeDefTokens.Add(type, token);
            return token;
        }

        private void AddTypeDefinitionMembers()
        {
            var table = Metadata.TablesStream.GetTable<TypeDefinitionRow>(TableIndex.TypeDef);

            uint fieldList = 1;
            uint methodList = 1;
            
            for (uint rid = 1; rid <= table.Count; rid++)
            {
                var row = table[rid];
                row = new TypeDefinitionRow(row.Attributes, row.Name, row.Namespace, row.Extends,
                    fieldList, methodList);
                table[rid] = row;

                var type = _typeDefTokens.GetKey(new MetadataToken(TableIndex.TypeDef, rid));
                fieldList += (uint) type.Fields.Count;
                methodList += (uint) type.Methods.Count;
            }
        }

        private MetadataToken AddMethodDefinition(MethodDefinition method)
        {
            var table = Metadata.TablesStream.GetTable<MethodDefinitionRow>(TableIndex.Method);
            
            var row = new MethodDefinitionRow(
                MethodBodySerializer.SerializeMethodBody(this, method), 
                method.ImplAttributes, 
                method.Attributes, 
                Metadata.StringsStream.GetStringIndex(method.Name),
                Metadata.BlobStream.GetBlobIndex(method.Signature),
                0);

            return table.Add(row);
        }
        
        private MetadataToken AddAssemblyReference(AssemblyReference reference)
        {
            AssertIsImported(reference);
            
            var table = Metadata.TablesStream.GetTable<AssemblyReferenceRow>(TableIndex.AssemblyRef);

            var row = new AssemblyReferenceRow((ushort) reference.Version.Major,
                (ushort) reference.Version.Minor,
                (ushort) reference.Version.Build,
                (ushort) reference.Version.Revision,
                reference.Attributes,
                Metadata.BlobStream.GetBlobIndex(reference.PublicKeyOrToken),
                Metadata.StringsStream.GetStringIndex(reference.Name),
                Metadata.StringsStream.GetStringIndex(reference.Culture),
                Metadata.BlobStream.GetBlobIndex(reference.HashValue));

            return table.Add(row);
        }

        private MetadataToken AddTypeReference(TypeReference type)
        {
            if (type == null)
                return 0;
            
            AssertIsImported(type);
            
            var table = Metadata.TablesStream.GetTable<TypeReferenceRow>(TableIndex.TypeRef);
            
            var row = new TypeReferenceRow(
                AddResolutionScope( type.Scope),
                Metadata.StringsStream.GetStringIndex(type.Name),
                Metadata.StringsStream.GetStringIndex(type.Namespace));

            return table.Add(row);
        }

        private MetadataToken AddTypeSpecification(TypeSpecification type)
        {
            AssertIsImported(type);
            
            var table = Metadata.TablesStream.GetTable<TypeSpecificationRow>(TableIndex.TypeSpec);
            var row = new TypeSpecificationRow(Metadata.BlobStream.GetBlobIndex(type.Signature));
            return table.Add(row);
        }
    }
}