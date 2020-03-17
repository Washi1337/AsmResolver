using System.IO;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Builder
{
    public partial class DotNetDirectoryBuffer
    {
        /// <summary>
        /// Adds an assembly, its entire manifest module, and all secondary module file references, to the buffer.
        /// </summary>
        /// <param name="assembly">The assembly to add.</param>
        public void AddAssembly(AssemblyDefinition assembly)
        {
            var table = Metadata.TablesStream.GetTable<AssemblyDefinitionRow>(TableIndex.Assembly);

            var row = new AssemblyDefinitionRow(
                assembly.HashAlgorithm,
                (ushort) assembly.Version.Major,
                (ushort) assembly.Version.Minor,
                (ushort) assembly.Version.Build,
                (ushort) assembly.Version.Revision,
                assembly.Attributes,
                Metadata.BlobStream.GetBlobIndex(assembly.PublicKey),
                Metadata.StringsStream.GetStringIndex(assembly.Name),
                Metadata.StringsStream.GetStringIndex(assembly.Culture));

            var token = table.Add(row, assembly.MetadataToken.Rid);
            AddCustomAttributes(token, assembly);
            AddModule(assembly.ManifestModule);
        }

        /// <summary>
        /// Adds a module and all its contents to the buffer. 
        /// </summary>
        /// <param name="module">The module to add.</param>
        public void AddModule(ModuleDefinition module)
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
            
            var token = table.Add(row, module.MetadataToken.Rid);
            
            // Ensure reference to corlib is added. 
            if (module.CorLibTypeFactory.CorLibScope is AssemblyReference corLibScope)
                GetAssemblyReferenceToken(corLibScope);
            
            AddCustomAttributes(token, module);
            AddTypeDefinitionsInModule(module);
            AddResourcesInModule(module);
        }

        private void AddResourcesInModule(ModuleDefinition module)
        {
            foreach (var resource in module.Resources)
                AddManifestResource(resource);
        }

        private MetadataToken AddManifestResource(ManifestResource resource)
        {
            uint offset = resource.Offset;
            if (resource.IsEmbedded)
            {
                using var stream = new MemoryStream();
                resource.EmbeddedDataSegment.Write(new BinaryStreamWriter(stream));
                offset = Resources.GetResourceDataOffset(stream.ToArray());
            }
            
            var table = Metadata.TablesStream.GetTable<ManifestResourceRow>(TableIndex.ManifestResource);
            var row = new ManifestResourceRow(
                offset,
                resource.Attributes,
                Metadata.StringsStream.GetStringIndex(resource.Name),
                AddImplementation(resource.Implementation));

            var token = table.Add(row, resource.MetadataToken.Rid);
            AddCustomAttributes(token, resource);
            return token;
        }

        private void AddTypeDefinitionsInModule(ModuleDefinition module)
        {
            AddTypeDefinitionStubs(module);
            AddTypeDefinitionMembers();
            FinalizeMethodDefinitions();
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
            
            AddCustomAttributes(token, type);
            AddInterfaces(token, type.Interfaces);
            AddGenericParameters(token, type);

            if (type.ClassLayout is {})
                AddClassLayout(token, type.ClassLayout);
            
            return token;
        }

        private void AddTypeDefinitionMembers()
        {
            var table = Metadata.TablesStream.GetTable<TypeDefinitionRow>(TableIndex.TypeDef);

            uint fieldList = 1;
            uint methodList = 1;
            uint propertyList = 1;
            uint eventList = 1;
            
            for (uint rid = 1; rid <= table.Count; rid++)
            {
                var type = _typeDefTokens.GetKey(new MetadataToken(TableIndex.TypeDef, rid));
                
                var row = table[rid];
                row = new TypeDefinitionRow(row.Attributes, row.Name, row.Namespace, 
                    AddTypeDefOrRef(type.BaseType), fieldList, methodList);
                table[rid] = row;
                
                foreach (var field in type.Fields)
                    AddFieldDefinition(field);
                foreach (var method in type.Methods)
                    AddMethodDefinitionStub(method);

                AddPropertyDefinitionsInType(type, rid, ref propertyList);
                AddEventDefinitionsInType(type, rid, ref eventList);
                
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
            AddCustomAttributes(token, field);
            AddConstant(token, field.Constant);
            AddImplementationMap(token, field.ImplementationMap);
            return token;
        }

        private MetadataToken AddMethodDefinitionStub(MethodDefinition method)
        {
            var table = Metadata.TablesStream.GetTable<MethodDefinitionRow>(TableIndex.Method);
            
            var row = new MethodDefinitionRow(
                null, 
                method.ImplAttributes, 
                method.Attributes, 
                Metadata.StringsStream.GetStringIndex(method.Name),
                Metadata.BlobStream.GetBlobIndex(this, method.Signature),
                0);

            var token = table.Add(row, method.MetadataToken.Rid);
            _methodTokens.Add(method, token);
            AddCustomAttributes(token, method);
            AddImplementationMap(token, method.ImplementationMap);
            AddGenericParameters(token, method);
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

            var token = table.Add(row, parameter.MetadataToken.Rid);
            AddCustomAttributes(token, parameter);
            AddConstant(token, parameter.Constant);
            return token;
        }

        private void AddPropertyDefinitionsInType(TypeDefinition type, uint typeRid, ref uint propertyList)
        {
            if (type.Properties.Count > 0)
            {
                var table = Metadata.TablesStream.GetTable<PropertyMapRow>(TableIndex.PropertyMap);
                    
                foreach (var property in type.Properties)
                    AddPropertyDefinition(property);
                
                var row = new PropertyMapRow(typeRid, propertyList);
                table.Add(row, 0);
                propertyList += (uint) type.Properties.Count;
            }
        }

        private MetadataToken AddPropertyDefinition(PropertyDefinition property)
        {
            var table = Metadata.TablesStream.GetTable<PropertyDefinitionRow>(TableIndex.Property);
            
            var row = new PropertyDefinitionRow(
                property.Attributes, 
                Metadata.StringsStream.GetStringIndex(property.Name),
                Metadata.BlobStream.GetBlobIndex(this, property.Signature));

            var token = table.Add(row, property.MetadataToken.Rid);
            AddCustomAttributes(token, property);
            AddMethodSemantics(token, property);
            AddConstant(token, property.Constant);
            return token;
        }

        private void AddEventDefinitionsInType(TypeDefinition type, uint typeRid, ref uint eventList)
        {
            if (type.Events.Count > 0)
            {
                var table = Metadata.TablesStream.GetTable<EventMapRow>(TableIndex.EventMap);
                    
                foreach (var @event in type.Events)
                    AddEventDefinition(@event);
                
                var row = new EventMapRow(typeRid, eventList);
                table.Add(row, 0);
                eventList += (uint) type.Events.Count;
            }
        }

        private MetadataToken AddEventDefinition(EventDefinition @event)
        {
            var table = Metadata.TablesStream.GetTable<EventDefinitionRow>(TableIndex.Event);
            
            var row = new EventDefinitionRow(
                @event.Attributes, 
                Metadata.StringsStream.GetStringIndex(@event.Name),
                AddTypeDefOrRef(@event.EventType));

            var token = table.Add(row, @event.MetadataToken.Rid);
            AddCustomAttributes(token, @event);
            AddMethodSemantics(token, @event);
            return token;
        }

        private void FinalizeMethodDefinitions()
        {
            var table = Metadata.TablesStream.GetTable<MethodDefinitionRow>(TableIndex.Method);
            for (uint rid = 1; rid <= table.Count; rid++)
            {
                var method = _methodTokens.GetKey(new MetadataToken(TableIndex.Method, rid));
                if (method.MethodBody != null)
                {
                    var row = table[rid];
                    row = new MethodDefinitionRow(
                        MethodBodySerializer.SerializeMethodBody(this, method),
                        row.ImplAttributes,
                        row.Attributes,
                        row.Name,
                        row.Signature,
                        row.ParameterList);
                    table[rid] = row;
                }
            }
        }

        private MetadataToken AddExportedType(ExportedType exportedType)
        {
            var table = Metadata.TablesStream.GetTable<ExportedTypeRow>(TableIndex.ExportedType);

            var row = new ExportedTypeRow(
                exportedType.Attributes,
                exportedType.TypeDefId,
                Metadata.StringsStream.GetStringIndex(exportedType.Name),
                Metadata.StringsStream.GetStringIndex(exportedType.Namespace),
                AddImplementation(exportedType.Implementation));

            var token = table.Add(row, exportedType.MetadataToken.Rid);
            AddCustomAttributes(token, exportedType);
            return token;
        }

        private MetadataToken AddFileReference(FileReference fileReference)
        {
            var table = Metadata.TablesStream.GetTable<FileReferenceRow>(TableIndex.File);

            var row = new FileReferenceRow(
                fileReference.Attributes,
                Metadata.StringsStream.GetStringIndex(fileReference.Name),
                Metadata.BlobStream.GetBlobIndex(fileReference.HashValue));

            var token = table.Add(row, fileReference.MetadataToken.Rid);
            AddCustomAttributes(token, fileReference);
            return token;
        }
    }
}