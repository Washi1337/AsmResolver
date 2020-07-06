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

            var token = table.Add(row);
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
            
            var token = table.Add(row);
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

            var token = table.Add(row);
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
            var nestedClassTable = Metadata.TablesStream.GetSortedTable<TypeDefinition, NestedClassRow>(TableIndex.NestedClass);
            
            foreach (var type in types)
            {
                // At this point, we might not have added all type defs/refs/specs yet, so we cannot determine
                // the extends column, nor determine the field and method lists of this type.
                
                var row = new TypeDefinitionRow(
                    type.Attributes,
                    Metadata.StringsStream.GetStringIndex(type.Name),
                    Metadata.StringsStream.GetStringIndex(type.Namespace),
                    0,
                    0,
                    0);

                var token = typeDefTable.Add(row);
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
                    
                    nestedClassTable.Add(type, nestedClassRow);
                }
            }
        }

        /// <summary>
        /// Allocates metadata rows for the provided field definitions in the buffer. 
        /// </summary>
        /// <param name="fields">The fields to define.</param>
        public void DefineFields(IEnumerable<FieldDefinition> fields)
        {
            var table = Metadata.TablesStream.GetTable<FieldDefinitionRow>(TableIndex.Field);

            foreach (var field in fields)
            {
                var row = new FieldDefinitionRow(
                    field.Attributes,
                    Metadata.StringsStream.GetStringIndex(field.Name),
                    Metadata.BlobStream.GetBlobIndex(this, field.Signature));

                var token = table.Add(row);
                _fieldTokens.Add(field, token);
            }
        }
        
        /// <summary>
        /// Allocates metadata rows for the provided method definitions in the buffer. 
        /// </summary>
        /// <param name="methods">The methods to define.</param>
        public void DefineMethods(IEnumerable<MethodDefinition> methods)
        {
            var table = Metadata.TablesStream.GetTable<MethodDefinitionRow>(TableIndex.Method);

            foreach (var method in methods)
            {
                // At this point, we might not have added all type defs/refs/specs yet, so we must delay the 
                // serialization of the method body, as well as determining the parameter list.
                
                var row = new MethodDefinitionRow(
                    null,
                    method.ImplAttributes,
                    method.Attributes,
                    Metadata.StringsStream.GetStringIndex(method.Name),
                    Metadata.BlobStream.GetBlobIndex(this, method.Signature),
                    0);

                var token = table.Add(row);
                _methodTokens.Add(method, token);
            }
        }

        /// <summary>
        /// Allocates metadata rows for the provided parameter definitions in the buffer. 
        /// </summary>
        /// <param name="parameters">The parameters to define.</param>
        public void DefineParameters(IEnumerable<ParameterDefinition> parameters)
        {
            var table = Metadata.TablesStream.GetTable<ParameterDefinitionRow>(TableIndex.Param);

            foreach (var parameter in parameters)
            {
                var row = new ParameterDefinitionRow(
                    parameter.Attributes,
                    parameter.Sequence,
                    Metadata.StringsStream.GetStringIndex(parameter.Name));

                var token = table.Add(row);
                _parameterTokens.Add(parameter, token);
            }
        }

        /// <summary>
        /// Allocates metadata rows for the provided property definitions in the buffer. 
        /// </summary>
        /// <param name="properties">The properties to define.</param>
        public void DefineProperties(IEnumerable<PropertyDefinition> properties)
        {
            var table = Metadata.TablesStream.GetTable<PropertyDefinitionRow>(TableIndex.Property);

            foreach (var property in properties)
            {
                var row = new PropertyDefinitionRow(
                    property.Attributes,
                    Metadata.StringsStream.GetStringIndex(property.Name),
                    Metadata.BlobStream.GetBlobIndex(this, property.Signature));
                
                var token = table.Add(row);
                _propertyTokens.Add(property, token);
            }
        }

        /// <summary>
        /// Allocates metadata rows for the provided event definitions in the buffer. 
        /// </summary>
        /// <param name="events">The events to define.</param>
        public void DefineEvents(IEnumerable<EventDefinition> events)
        {
            var table = Metadata.TablesStream.GetTable<EventDefinitionRow>(TableIndex.Event);

            foreach (var @event in events)
            {
                var row = new EventDefinitionRow(
                    @event.Attributes, 
                    Metadata.StringsStream.GetStringIndex(@event.Name),
                    GetTypeDefOrRefIndex(@event.EventType));
                
                var token = table.Add(row);
                _eventTokens.Add(@event, token);
            }
        }
        
        /// <summary>
        /// Finalizes all type definitions added in the buffer.
        /// </summary>
        public void FinalizeTypes()
        {
            var typeDefTable = Metadata.TablesStream.GetTable<TypeDefinitionRow>(TableIndex.TypeDef);
            
            bool fieldPtrRequired = false;
            bool methodPtrRequired = false;
            bool paramPtrRequired = false;
            bool propertyPtrRequired = false;
            bool eventPtrRequired = false;

            uint fieldList = 1;
            uint methodList = 1;
            uint paramList = 1;
            uint propertyList = 1;
            uint eventList = 1;

            for (uint rid = 1; rid <= typeDefTable.Count; rid++)
            {
                var typeToken = new MetadataToken(TableIndex.TypeDef, rid);
                var type = _typeDefTokens.GetKey(typeToken);

                // Update extends, field list and method list columns.
                var typeRow = typeDefTable[rid];
                typeDefTable[rid] = new TypeDefinitionRow(
                    typeRow.Attributes,
                    typeRow.Name,
                    typeRow.Namespace,
                    GetTypeDefOrRefIndex(type.BaseType),
                    fieldList,
                    methodList);

                // Finalize fields and methods.
                FinalizeFieldsInType(type, ref fieldPtrRequired);
                FinalizeMethodsInType(type, ref methodPtrRequired, ref paramList, ref paramPtrRequired);
                FinalizePropertiesInType(type, rid, ref propertyList, ref propertyPtrRequired);
                FinalizeEventsInType(type, rid, ref eventList, ref eventPtrRequired);
                
                // Move to next ember lists.
                fieldList += (uint) type.Fields.Count;
                methodList += (uint) type.Methods.Count;

                // Add remaining metadata:
                AddCustomAttributes(typeToken, type);
                AddSecurityDeclarations(typeToken, type);
                DefineInterfaces(typeToken, type.Interfaces);
                AddMethodImplementations(typeToken, type.MethodImplementations);
                DefineGenericParameters(typeToken, type);
                AddClassLayout(typeToken, type.ClassLayout);
            }
            
            // Check if any of the redirection tables can be removed.
            if (!fieldPtrRequired)
                Metadata.TablesStream.GetTable<FieldPointerRow>(TableIndex.FieldPtr).Clear();
            if (!methodPtrRequired)
                Metadata.TablesStream.GetTable<MethodPointerRow>(TableIndex.MethodPtr).Clear();
            if (!paramPtrRequired)
                Metadata.TablesStream.GetTable<ParameterPointerRow>(TableIndex.ParamPtr).Clear();
            if (!propertyPtrRequired)
                Metadata.TablesStream.GetTable<PropertyPointerRow>(TableIndex.PropertyPtr).Clear();
            if (!eventPtrRequired)
                Metadata.TablesStream.GetTable<EventPointerRow>(TableIndex.EventPtr).Clear();
        }

        private void FinalizeFieldsInType(TypeDefinition type, ref bool fieldPtrRequired)
        {
            var pointerTable = Metadata.TablesStream.GetTable<FieldPointerRow>(TableIndex.FieldPtr);
            
            for (int i = 0; i < type.Fields.Count; i++)
            {
                var field = type.Fields[i];
                
                var newToken = GetFieldDefinitionToken(field);
                if (newToken == MetadataToken.Zero)
                {
                    throw new InvalidOperationException(
                        $"An attempt was made to finalize field {field}, which was not added to the .NET directory buffer yet.");
                }
                
                // Add field pointer row, making sure the RID is preserved.
                // We only really need the field pointer table if the next RID is not the RID that we would expect
                // from a normal sequential layout of the table.
                if (newToken.Rid != pointerTable.Count + 1)
                    fieldPtrRequired = true;
                pointerTable.Add(new FieldPointerRow(newToken.Rid));

                AddCustomAttributes(newToken, field);
                AddConstant(newToken, field.Constant);
                AddImplementationMap(newToken, field.ImplementationMap);
                AddFieldRva(newToken, field);
                AddFieldLayout(newToken, field);
                AddFieldMarshal(newToken, field);
            }
        }
        
        private void FinalizeMethodsInType(
            TypeDefinition type,
            ref bool methodPtrRequired,
            ref uint paramList, 
            ref bool paramPtrRequired)
        {
            var definitionTable = Metadata.TablesStream.GetTable<MethodDefinitionRow>(TableIndex.Method);
            var pointerTable = Metadata.TablesStream.GetTable<MethodPointerRow>(TableIndex.MethodPtr);
            
            for (int i = 0; i < type.Methods.Count; i++)
            {
                var method = type.Methods[i];
                
                var newToken = GetMethodDefinitionToken(method);
                if (newToken == MetadataToken.Zero)
                {
                    throw new InvalidOperationException(
                        $"An attempt was made to finalize method {method}, which was not added to the .NET directory buffer yet.");
                }
                
                // Add method pointer row, making sure the RID is preserved.
                // We only really need the method pointer table if the next RID is not the RID that we would expect
                // from a normal sequential layout of the table.
                if (newToken.Rid != pointerTable.Count + 1)
                    methodPtrRequired = true;
                pointerTable.Add(new MethodPointerRow(newToken.Rid));
                
                // Serialize method body and update column.
                var row = definitionTable[newToken.Rid];
                definitionTable[newToken.Rid] = new MethodDefinitionRow(
                    MethodBodySerializer.SerializeMethodBody(this, method),
                    row.ImplAttributes,
                    row.Attributes,
                    row.Name,
                    row.Signature,
                    paramList);
            
                // Finalize parameters.
                FinalizeParametersInMethod(method, ref paramList, ref paramPtrRequired);

                // Add remaining metadata.
                AddCustomAttributes(newToken, method);
                AddSecurityDeclarations(newToken, method);
                AddImplementationMap(newToken, method.ImplementationMap);
                DefineGenericParameters(newToken, method);
            }
        }

        private void FinalizeParametersInMethod(MethodDefinition method, ref uint paramList, ref bool paramPtrRequired)
        {
            var pointerTable = Metadata.TablesStream.GetTable<ParameterPointerRow>(TableIndex.ParamPtr);
            
            for (int i = 0; i < method.ParameterDefinitions.Count; i++)
            {
                var parameter = method.ParameterDefinitions[i];

                var newToken = GetParameterDefinitionToken(parameter);
                if (newToken == MetadataToken.Zero)
                {
                    throw new MetadataBuilderException(
                        $"An attempt was made to finalize parameter {parameter} in {method}, which was not added to the .NET directory buffer yet.");
                }
                
                // Add parameter pointer row, making sure the RID is preserved.
                // We only really need the parameter pointer table if the next RID is not the RID that we would expect
                // from a normal sequential layout of the table.
                if (newToken.Rid != pointerTable.Count + 1)
                    paramPtrRequired = true;
                pointerTable.Add(new ParameterPointerRow(newToken.Rid));
                
                // Add remaining metadata.
                AddCustomAttributes(newToken, parameter);
                AddConstant(newToken, parameter.Constant);
                AddFieldMarshal(newToken, parameter);
            }

            paramList += (uint) method.ParameterDefinitions.Count;
        }
        
        private void FinalizePropertiesInType(TypeDefinition type, uint typeRid, ref uint propertyList,
            ref bool propertyPtrRequired)
        {
            // Don't emit property map rows when the type does not define any properties.
            if (type.Properties.Count == 0)
                return;
            
            var mapTable = Metadata.TablesStream.GetSortedTable<TypeDefinition, PropertyMapRow>(TableIndex.PropertyMap);
            var pointerTable = Metadata.TablesStream.GetTable<PropertyPointerRow>(TableIndex.PropertyPtr);

            for (int i = 0; i < type.Properties.Count; i++)
            {
                var property = type.Properties[i];
                
                var newToken = GetPropertyDefinitionToken(property);
                if (newToken == MetadataToken.Zero)
                {
                    throw new MetadataBuilderException(
                        $"An attempt was made to finalize property {property}, which was not added to the .NET directory buffer yet.");
                }

                // Add property pointer row, making sure the RID is preserved.
                // We only really need the property pointer table if the next RID is not the RID that we would expect
                // from a normal sequential layout of the table.
                if (newToken.Rid != pointerTable.Count + 1)
                    propertyPtrRequired = true;
                pointerTable.Add(new PropertyPointerRow(newToken.Rid));
                
                // Add remaining metadata.
                AddCustomAttributes(newToken, property);
                AddMethodSemantics(newToken, property);
                AddConstant(newToken, property.Constant);
            }

            // Map the type to the property list.
            var row = new PropertyMapRow(typeRid, propertyList);
            mapTable.Add(type, row);
            propertyList += (uint) type.Properties.Count;
        }

        private void FinalizeEventsInType(TypeDefinition type, uint typeRid, ref uint eventList, ref bool eventPtrRequired)
        {
            // Don't emit event map rows when the type does not define any properties.
            if (type.Events.Count == 0)
                return;
            
            var mapTable = Metadata.TablesStream.GetSortedTable<TypeDefinition, EventMapRow>(TableIndex.EventMap);
            var pointerTable = Metadata.TablesStream.GetTable<EventPointerRow>(TableIndex.EventPtr);
            
            for (int i = 0; i < type.Events.Count; i++)
            {
                var @event = type.Events[i];
                
                var newToken = GetEventDefinitionToken(@event);
                if (newToken == MetadataToken.Zero)
                {
                    throw new MetadataBuilderException(
                        $"An attempt was made to finalize event {@event}, which was not added to the .NET directory buffer yet.");
                }
                
                // Add event pointer row, making sure the RID is preserved.
                // We only really need the event pointer table if the next RID is not the RID that we would expect
                // from a normal sequential layout of the table.
                if (newToken.Rid != pointerTable.Count + 1)
                    eventPtrRequired = true;
                pointerTable.Add(new EventPointerRow(newToken.Rid));
                
                // Add remaining metadata.
                AddCustomAttributes(newToken, @event);
                AddMethodSemantics(newToken, @event);
            }
            
            // Map the type to the event list.
            var row = new EventMapRow(typeRid, eventList);
            mapTable.Add(type, row);
            eventList += (uint) type.Events.Count;
        }

        private void AddMethodImplementations(MetadataToken typeToken, IList<MethodImplementation> implementations)
        {
            var table = Metadata.TablesStream.GetSortedTable<MethodImplementation, MethodImplementationRow>(TableIndex.MethodImpl);

            for (int i = 0; i < implementations.Count; i++)
            {
                var implementation = implementations[i];
                var row = new MethodImplementationRow(
                    typeToken.Rid,
                    AddMethodDefOrRef(implementation.Body),
                    AddMethodDefOrRef(implementation.Declaration));

                table.Add(implementation, row);
            }
        }

        private void AddFieldRva(MetadataToken ownerToken, FieldDefinition field)
        {
            // Don't emit any field rva rows if no initial data was specified.
            if (field.FieldRva is null)
                return;
            
            var table = Metadata.TablesStream.GetSortedTable<FieldDefinition, FieldRvaRow>(TableIndex.FieldRva);
            
            var row = new FieldRvaRow(
                new SegmentReference(field.FieldRva), 
                ownerToken.Rid);

            table.Add(field, row);
        }

        private void AddFieldLayout(MetadataToken ownerToken, FieldDefinition field)
        {
            // Don't emit any field offset rows if no initial data was specified.
            if (!field.FieldOffset.HasValue)
                return;
            
            var table = Metadata.TablesStream.GetSortedTable<FieldDefinition, FieldLayoutRow>(TableIndex.FieldLayout);
            
            var row = new FieldLayoutRow(
                (uint) field.FieldOffset.Value, 
                ownerToken.Rid);

            table.Add(field, row);
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

            var token = table.Add(row);
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

            var token = table.Add(row);
            AddCustomAttributes(token, fileReference);
            return token;
        }
    }
}