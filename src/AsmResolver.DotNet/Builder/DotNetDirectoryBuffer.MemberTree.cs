using AsmResolver.DotNet.Collections;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Builder
{
    public partial class DotNetDirectoryBuffer
    {
        private readonly OneToOneRelation<TypeDefinition, MetadataToken> _typeDefTokens = new OneToOneRelation<TypeDefinition, MetadataToken>();
        private readonly OneToOneRelation<MethodDefinition, MetadataToken> _methodTokens = new OneToOneRelation<MethodDefinition, MetadataToken>();
        private readonly OneToOneRelation<FieldDefinition, MetadataToken> _fieldTokens = new OneToOneRelation<FieldDefinition, MetadataToken>();

        public MetadataToken GetTypeDefinitionToken(TypeDefinition type)
        {
            AssertIsImported(type);
            return _typeDefTokens.GetValue(type);
        }

        public MetadataToken GetFieldDefinitionToken(FieldDefinition field)
        {
            AssertIsImported(field);
            return _fieldTokens.GetValue(field);
        }

        public MetadataToken GetMethodDefinitionToken(MethodDefinition method)
        {
            AssertIsImported(method);
            return _methodTokens.GetValue(method);
        }

        public void AddManifestModule(ModuleDefinition module)
        {
            var stringsStream = Metadata.StringsStream;
            var guidStream = Metadata.GuidStream;

            var table = Metadata.TablesStream.GetTable<ModuleDefinitionRow>(TableIndex.Module);
            var row = new ModuleDefinitionRow(
                module.Generation,
                stringsStream.GetStringIndex(module.Name),
                guidStream.GetGuidIndex(module.Mvid),
                guidStream.GetGuidIndex(module.EncId),
                guidStream.GetGuidIndex(module.EncBaseId));
            table.Add(row, module.MetadataToken.Rid);

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

            var token = table.Add(row, type.MetadataToken.Rid);
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
                
                foreach (var field in type.Fields)
                    AddFieldDefinition(field);
                foreach (var method in type.Methods)
                    AddMethodDefinitionStub(method);
                
                fieldList += (uint) type.Fields.Count;
                methodList += (uint) type.Methods.Count;
            }
            
            AddParameterDefinitions();
        }

        private MetadataToken AddFieldDefinition(FieldDefinition field)
        {
            var table = Metadata.TablesStream.GetTable<FieldDefinitionRow>(TableIndex.Field);

            var row = new FieldDefinitionRow(
                field.Attributes,
                Metadata.StringsStream.GetStringIndex(field.Name),
                Metadata.BlobStream.GetBlobIndex(this, field.Signature));

            var token = table.Add(row, field.MetadataToken.Rid);
            _fieldTokens.Add(field, token);
            return token;
        }

        private MetadataToken AddMethodDefinitionStub(MethodDefinition method)
        {
            var table = Metadata.TablesStream.GetTable<MethodDefinitionRow>(TableIndex.Method);
            
            var row = new MethodDefinitionRow(
                MethodBodySerializer.SerializeMethodBody(this, method), 
                method.ImplAttributes, 
                method.Attributes, 
                Metadata.StringsStream.GetStringIndex(method.Name),
                Metadata.BlobStream.GetBlobIndex(this, method.Signature),
                0);

            var token = table.Add(row, method.MetadataToken.Rid);
            _methodTokens.Add(method, token);
            return token;
        }

        private void AddParameterDefinitions()
        {
            var table = Metadata.TablesStream.GetTable<MethodDefinitionRow>(TableIndex.Method);

            uint paramList = 1;
            
            for (uint rid = 1; rid <= table.Count; rid++)
            {
                var row = table[rid];
                row = new MethodDefinitionRow(row.Body, row.ImplAttributes, row.Attributes, row.Name, row.Signature,
                    paramList);
                table[rid] = row;

                var method = _methodTokens.GetKey(new MetadataToken(TableIndex.Method, rid));
                
                foreach (var parameter in method.ParameterDefinitions)
                    AddParameterDefinition(parameter);
                
                paramList += (uint) method.ParameterDefinitions.Count;
            }
        }

        private MetadataToken AddParameterDefinition(ParameterDefinition parameter)
        {
            var table = Metadata.TablesStream.GetTable<ParameterDefinitionRow>(TableIndex.Param);
            
            var row = new ParameterDefinitionRow(
                parameter.Attributes,
                parameter.Sequence,
                Metadata.StringsStream.GetStringIndex(parameter.Name));

            return table.Add(row, parameter.MetadataToken.Rid);
        }
    }
}