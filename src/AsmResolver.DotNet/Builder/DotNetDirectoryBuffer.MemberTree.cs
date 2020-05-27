using System;
using System.Collections.Generic;
using System.IO;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Builder
{
    public partial class DotNetDirectoryBuffer
    {
        private readonly IDictionary<ModuleDefinition, MetadataToken> _moduleDefTokens =
            new Dictionary<ModuleDefinition, MetadataToken>();
        
        /// <summary>
        /// Adds an assembly, its entire manifest module, and all secondary module file references, to the buffer.
        /// </summary>
        /// <param name="assembly">The assembly to add.</param>
        public void DefineAssembly(AssemblyDefinition assembly)
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
            AddSecurityDeclarations(token, assembly);
        }

        /// <summary>
        /// Adds a module metadata row to the buffer. 
        /// </summary>
        /// <param name="module">The module to add.</param>
        /// <remarks>
        /// This method only adds the metadata row of the module definition to the module table buffer,
        /// it does not add any type definition to the buffer, nor does it add custom attributes or any
        /// other metadata model object related to this module to the buffer.
        /// </remarks>
        public void DefineModule(ModuleDefinition module)
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
            _moduleDefTokens[module] = token;
        }

        /// <summary>
        /// Finalizes the module definition.
        /// </summary>
        /// <param name="module">The module to finalize.</param>
        public void FinalizeModule(ModuleDefinition module)
        {
            var token = _moduleDefTokens[module];
            
            // Ensure reference to corlib is added. 
            if (module.CorLibTypeFactory.CorLibScope is AssemblyReference corLibScope)
                GetAssemblyReferenceToken(corLibScope);

            AddResourcesInModule(module);
            AddCustomAttributes(token, module);
        }

        private void AddResourcesInModule(ModuleDefinition module)
        {
            foreach (var resource in module.Resources)
                AddManifestResource(resource);
        }

        /// <summary>
        /// Adds a single manifest resource to the buffer.
        /// </summary>
        /// <param name="resource">The resource to add.</param>
        /// <returns>The new metadata token of the resource.</returns>
        public MetadataToken AddManifestResource(ManifestResource resource)
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
        
        /// <summary>
        /// Allocates metadata rows for the provided type definitions in the buffer.  
        /// </summary>
        /// <param name="types">The types to define.</param>
        /// <remarks>
        /// This method does not define any member defined in the type, except for nested types.
        /// </remarks>
        public void DefineTypes(IEnumerable<TypeDefinition> types)
        {
            var typeDefTable = Metadata.TablesStream.GetTable<TypeDefinitionRow>(TableIndex.TypeDef);
            var nestedClassTable = Metadata.TablesStream.GetTable<NestedClassRow>(TableIndex.NestedClass);
            
            foreach (var type in types)
            {
                var row = new TypeDefinitionRow(
                    type.Attributes,
                    Metadata.StringsStream.GetStringIndex(type.Name),
                    Metadata.StringsStream.GetStringIndex(type.Namespace),
                    0,
                    0,
                    0);

                var token = typeDefTable.Add(row, type.MetadataToken.Rid);
                _typeDefTokens.Add(type, token);

                if (type.IsNested)
                {
                    // As per the ECMA-335; nested types should always follow their enclosing types in the TypeDef table.
                    // Proper type def collections that are passed onto this function therefore should have been added
                    // already to the buffer. If not, we have an invalid ordering of types.
                    
                    var enclosingTypeToken = GetTypeDefinitionToken(type.DeclaringType);
                    if (enclosingTypeToken.Rid ==0)
                        throw new ArgumentException($"Nested type {type.FullName} is added before enclosing class.");
                    
                    var nestedClassRow = new NestedClassRow(
                        token.Rid,
                        enclosingTypeToken.Rid);
                    
                    nestedClassTable.Add(nestedClassRow, 0);
                }
            }
        }

        /// <summary>
        /// Allocates metadata rows for all fields and methods defined in the types that are added to the buffer. 
        /// </summary>
        public void DefineMembersInTypes()
        {
            var table = Metadata.TablesStream.GetTable<TypeDefinitionRow>(TableIndex.TypeDef);

            uint fieldList = 1;
            uint methodList = 1;
            for (uint rid = 1; rid <= table.Count; rid++)
            {
                var typeToken = new MetadataToken(TableIndex.TypeDef, rid);
                var type = _typeDefTokens.GetKey(typeToken);
                
                var row = table[rid];
                row = new TypeDefinitionRow(row.Attributes, row.Name, row.Namespace,
                    GetTypeDefOrRefIndex(type.BaseType), fieldList, methodList);
                table[rid] = row;

                foreach (var field in type.Fields)
                    DefineField(field);
                foreach (var method in type.Methods)
                    DefineMethod(method);

                fieldList += (uint) type.Fields.Count;
                methodList += (uint) type.Methods.Count;
            }
        }

        private void DefineField(FieldDefinition field)
        {
            var table = Metadata.TablesStream.GetTable<FieldDefinitionRow>(TableIndex.Field);

            var row = new FieldDefinitionRow(
                field.Attributes,
                Metadata.StringsStream.GetStringIndex(field.Name),
                Metadata.BlobStream.GetBlobIndex(this, field.Signature));

            var token = table.Add(row, field.MetadataToken.Rid);
            _fieldTokens.Add(field, token);
        }
        
        private void DefineMethod(MethodDefinition method)
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
        }
        
        /// <summary>
        /// Finalizes all type definitions added in the buffer.
        /// </summary>
        public void FinalizeTypes()
        {
            var table = Metadata.TablesStream.GetTable<TypeDefinitionRow>(TableIndex.TypeDef);

            uint propertyList = 1;
            uint eventList = 1;

            for (uint rid = 1; rid <= table.Count; rid++)
            {
                var typeToken = new MetadataToken(TableIndex.TypeDef, rid);
                var type = _typeDefTokens.GetKey(typeToken);
                
                AddPropertiesInType(type, rid, ref propertyList);
                AddEventsInType(type, rid, ref eventList);
                
                AddCustomAttributes(typeToken, type);
                AddSecurityDeclarations(typeToken, type);
                AddInterfaces(typeToken, type.Interfaces);
                AddMethodImplementations(typeToken, type.MethodImplementations);
                AddGenericParameters(typeToken, type);
                AddClassLayout(typeToken, type.ClassLayout);
            }
            
            FinalizeFields();
            FinalizeMethods();
            AddParameters();
        }
        
        private void AddMethodImplementations(MetadataToken typeToken, IList<MethodImplementation> methodImplementations)
        {
            var table = Metadata.TablesStream.GetTable<MethodImplementationRow>(TableIndex.MethodImpl);

            foreach (var implementation in methodImplementations)
            {
                var row = new MethodImplementationRow(
                    typeToken.Rid,
                    AddMethodDefOrRef(implementation.Body),
                    AddMethodDefOrRef(implementation.Declaration));

                table.Add(row, 0);
            }
        }

        private void AddPropertiesInType(TypeDefinition type, uint typeRid, ref uint propertyList)
        {
            if (type.Properties.Count > 0)
            {
                var table = Metadata.TablesStream.GetTable<PropertyMapRow>(TableIndex.PropertyMap);
                    
                foreach (var property in type.Properties)
                    AddProperty(property);
                
                var row = new PropertyMapRow(typeRid, propertyList);
                table.Add(row, 0);
                propertyList += (uint) type.Properties.Count;
            }
        }

        private MetadataToken AddProperty(PropertyDefinition property)
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

        private void AddEventsInType(TypeDefinition type, uint typeRid, ref uint eventList)
        {
            if (type.Events.Count > 0)
            {
                var table = Metadata.TablesStream.GetTable<EventMapRow>(TableIndex.EventMap);
                    
                foreach (var @event in type.Events)
                    AddEvent(@event);
                
                var row = new EventMapRow(typeRid, eventList);
                table.Add(row, 0);
                eventList += (uint) type.Events.Count;
            }
        }

        private MetadataToken AddEvent(EventDefinition @event)
        {
            var table = Metadata.TablesStream.GetTable<EventDefinitionRow>(TableIndex.Event);
            
            var row = new EventDefinitionRow(
                @event.Attributes, 
                Metadata.StringsStream.GetStringIndex(@event.Name),
                GetTypeDefOrRefIndex(@event.EventType));

            var token = table.Add(row, @event.MetadataToken.Rid);
            AddCustomAttributes(token, @event);
            AddMethodSemantics(token, @event);
            return token;
        }

        private void FinalizeFields()
        {
            var table = Metadata.TablesStream.GetTable<FieldDefinitionRow>(TableIndex.Field);
            for (uint rid = 1; rid <= table.Count; rid++)
            {
                var fieldToken = new MetadataToken(TableIndex.Field, rid);
                var field = _fieldTokens.GetKey(fieldToken);
                
                AddCustomAttributes(fieldToken, field);
                AddConstant(fieldToken, field.Constant);
                AddImplementationMap(fieldToken, field.ImplementationMap);
                AddFieldRva(fieldToken, field.FieldRva);
                AddFieldLayout(fieldToken, field.FieldOffset);
            }
        }

        private void AddFieldRva(MetadataToken ownerToken, ISegment fieldRva)
        {
            if (fieldRva is null)
                return;
            
            var table = Metadata.TablesStream.GetTable<FieldRvaRow>(TableIndex.FieldRva);
            
            var row = new FieldRvaRow(
                new SegmentReference(fieldRva), 
                ownerToken.Rid);

            table.Add(row, 0);
        }

        private void AddFieldLayout(MetadataToken ownerToken, int? fieldOffset)
        {
            if (fieldOffset is null)
                return;
            
            var table = Metadata.TablesStream.GetTable<FieldLayoutRow>(TableIndex.FieldLayout);
            
            var row = new FieldLayoutRow(
                (uint) fieldOffset.Value, 
                ownerToken.Rid);

            table.Add(row, 0);
        }
        
        private void FinalizeMethods()
        {
            var table = Metadata.TablesStream.GetTable<MethodDefinitionRow>(TableIndex.Method);
            for (uint rid = 1; rid <= table.Count; rid++)
            {
                var methodToken = new MetadataToken(TableIndex.Method, rid);
                var method = _methodTokens.GetKey(methodToken);
                
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
                
                AddCustomAttributes(methodToken, method);
                AddSecurityDeclarations(methodToken, method);
                AddImplementationMap(methodToken, method.ImplementationMap);
                AddGenericParameters(methodToken, method);
            }
        }
        
        private void AddParameters()
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
                    AddParameter(parameter);
                
                paramList += (uint) method.ParameterDefinitions.Count;
            }
        }

        private MetadataToken AddParameter(ParameterDefinition parameter)
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