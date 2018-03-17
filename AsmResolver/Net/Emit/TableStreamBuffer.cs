using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Emit
{
    public class TableStreamBuffer : MetadataStreamBuffer
    {
        private sealed class MetadataRowComparer : IEqualityComparer<MetadataRow>
        {
            public bool Equals(MetadataRow x, MetadataRow y)
            {
                return x.GetAllColumns().SequenceEqual(y.GetAllColumns());
            }

            public int GetHashCode(MetadataRow obj)
            {
                return obj.GetAllColumns().Aggregate(0, (current, column) => current ^ column.GetHashCode());
            }
        }
        
        private readonly MetadataBuffer _parentBuffer;

        private readonly IDictionary<IMetadataMember, MetadataToken> _members =
            new Dictionary<IMetadataMember, MetadataToken>();
        
        private readonly IDictionary<MetadataRow<uint, uint, uint>, MetadataToken> _typeRefs;
        private readonly IDictionary<MetadataRow<uint>, MetadataToken> _typeSpecs;
        private readonly IDictionary<MetadataRow<ushort, ushort, ushort, ushort, AssemblyAttributes, uint, uint, uint, uint>, MetadataToken> _assemblyRefs;
        private readonly IDictionary<MetadataRow<uint>, MetadataToken> _moduleRefs;        
        private readonly IDictionary<MetadataRow<uint, uint, uint>, MetadataToken> _memberRefs;
        private readonly IDictionary<MetadataRow<uint, uint>, MetadataToken> _methodSpecs;

        private readonly TableStream _tableStream;

        private uint _methodList = 1;
        private uint _fieldList = 1;
        private uint _paramList = 1;
        private uint _propertyList = 1;
        private uint _eventList = 1;

        public TableStreamBuffer(MetadataBuffer parentBuffer)
        {
            _parentBuffer = parentBuffer;
            _tableStream = new TableStream();

            var rowComparer = new MetadataRowComparer();
            _typeRefs = new Dictionary<MetadataRow<uint, uint, uint>, MetadataToken>(rowComparer);
            _typeSpecs = new Dictionary<MetadataRow<uint>, MetadataToken>(rowComparer);
            _assemblyRefs = new Dictionary<MetadataRow<ushort, ushort, ushort, ushort, AssemblyAttributes, uint, uint, uint, uint>, MetadataToken>(rowComparer);
            _moduleRefs = new Dictionary<MetadataRow<uint>, MetadataToken>(rowComparer);
            _memberRefs = new Dictionary<MetadataRow<uint, uint, uint>, MetadataToken>(rowComparer);
            _methodSpecs = new Dictionary<MetadataRow<uint, uint>, MetadataToken>(rowComparer);
        }

        public override string Name
        {
            get { return "#~"; }
        }

        public override uint Length
        {
            get { return 0; }
        }

        public IDictionary<IMetadataMember, MetadataToken> GetNewTokenMapping()
        {
            return _members;
        }
        
        private void AssertIsImported(IMetadataMember member)
        {
            if (member.Image != _parentBuffer.Image)
                throw new MemberNotImportedException(member);
        }

        public IndexEncoder GetIndexEncoder(CodedIndex codedIndex)
        {
            return _tableStream.GetIndexEncoder(codedIndex);
        }

        public MetadataToken GetNewToken(IMetadataMember member)
        {
            MetadataToken token;
            if (!_members.TryGetValue(member, out token))
                throw new MemberNotImportedException(member);
            return token;
        }

        public MetadataToken GetTypeToken(ITypeDefOrRef type)
        {
            var definition = type as TypeDefinition;
            if (definition != null)
                return GetNewToken(definition);

            var reference = type as TypeReference;
            if (reference != null)
                return GetTypeReferenceToken(reference);

            var specification = type as TypeSpecification;
            if (specification != null)
                return GetTypeSpecificationToken(specification);
            
            throw new NotSupportedException("Invalid or unsupported TypeDefOrRef reference " + type + ".");
        }

        public MetadataToken GetTypeReferenceToken(TypeReference reference)
        {
            MetadataToken token;
            if (_members.TryGetValue(reference, out token))
                return token;
            
            AssertIsImported(reference);
            
            var typeRefRow = new MetadataRow<uint, uint, uint>
            {
                Column1 = _tableStream.GetIndexEncoder(CodedIndex.ResolutionScope)
                    .EncodeToken(GetResolutionScopeToken(reference.ResolutionScope)),
                Column2 = _parentBuffer.StringStreamBuffer.GetStringOffset(reference.Name),
                Column3 = _parentBuffer.StringStreamBuffer.GetStringOffset(reference.Namespace),
            };

            if (!_typeRefs.TryGetValue(typeRefRow, out token))
            {
                var table = (TypeReferenceTable) _tableStream.GetTable(MetadataTokenType.TypeRef);
                table.Add(typeRefRow);
                token = typeRefRow.MetadataToken;
                _typeRefs.Add(typeRefRow, token);
                _members.Add(reference, token);
                
                AddCustomAttributes(reference);
            }

            return token;
        }

        public MetadataToken GetTypeSpecificationToken(TypeSpecification specification)
        {
            MetadataToken token;
            if (_members.TryGetValue(specification, out token))
                return token;
            
            AssertIsImported(specification);
            
            var typeSpecRow = new MetadataRow<uint>
            {
                Column1 = _parentBuffer.BlobStreamBuffer.GetBlobOffset(specification.Signature)
            };

            if (!_typeSpecs.TryGetValue(typeSpecRow, out token))
            {
                var table = (TypeSpecificationTable) _tableStream.GetTable(MetadataTokenType.TypeSpec);
                table.Add(typeSpecRow);
                token = typeSpecRow.MetadataToken;
                _typeSpecs.Add(typeSpecRow, token);
                _members.Add(specification, token);
                
                AddCustomAttributes(specification);
            }

            return token;
        }

        public MetadataToken GetResolutionScopeToken(IResolutionScope scope)
        {
            var assemblyRef = scope as AssemblyReference;
            if (assemblyRef != null)
                return GetAssemblyReferenceToken(assemblyRef);

            var moduleDef = scope as ModuleDefinition;
            if (moduleDef != null)
                return GetNewToken(moduleDef);

            var moduleRef = scope as ModuleReference;
            if (moduleRef != null)
                return GetModuleReferenceToken(moduleRef);

            var typeRef = scope as TypeReference;
            if (typeRef != null)
                return GetTypeReferenceToken(typeRef);
            
            throw new NotSupportedException("Invalid or unsupported ResolutionScope reference " + scope + ".");
        }

        public MetadataToken GetAssemblyReferenceToken(AssemblyReference assemblyRef)
        {
            MetadataToken token;
            if (_members.TryGetValue(assemblyRef, out token))
                return token;
            
            AssertIsImported(assemblyRef);
            
            var assemblyRow =
                new MetadataRow<ushort, ushort, ushort, ushort, AssemblyAttributes, uint, uint, uint, uint>
                {
                    Column1 = (ushort) assemblyRef.Version.Major,
                    Column2 = (ushort) assemblyRef.Version.Minor,
                    Column3 = (ushort) assemblyRef.Version.Build,
                    Column4 = (ushort) assemblyRef.Version.Revision,
                    Column5 = assemblyRef.Attributes,
                    Column6 = _parentBuffer.BlobStreamBuffer.GetBlobOffset(assemblyRef.PublicKey),
                    Column7 = _parentBuffer.StringStreamBuffer.GetStringOffset(assemblyRef.Name),
                    Column8 = _parentBuffer.StringStreamBuffer.GetStringOffset(assemblyRef.Culture == "neutral" ? null : assemblyRef.Culture),
                    Column9 = _parentBuffer.BlobStreamBuffer.GetBlobOffset(assemblyRef.HashValue),
                };

            if (!_assemblyRefs.TryGetValue(assemblyRow, out token))
            {
                var table = (AssemblyReferenceTable) _tableStream.GetTable(MetadataTokenType.AssemblyRef);
                table.Add(assemblyRow);
                token = assemblyRow.MetadataToken;
                _assemblyRefs.Add(assemblyRow, token);
                _members.Add(assemblyRef, token);
                
                AddCustomAttributes(assemblyRef);
                
                // TODO: add legacy os and processors.
            }

            return token;
        }

        public MetadataToken GetMethodToken(IMethodDefOrRef method)
        {
            var definition = method as MethodDefinition;
            if (definition != null)
                return GetNewToken(definition);

            var reference = method as MemberReference;
            if (reference != null)
                return GetMemberReferenceToken(reference);
            
            throw new NotSupportedException("Invalid or unsupported MethodDefOrRef reference + " + method + ".");
        }

        public MetadataToken GetMemberReferenceToken(MemberReference reference)
        {
            MetadataToken token;
            if (_members.TryGetValue(reference, out token))
                return token;
            
            AssertIsImported(reference);

            var memberRow = new MetadataRow<uint, uint, uint>
            {
                Column1 = _tableStream.GetIndexEncoder(CodedIndex.MemberRefParent).EncodeToken(GetMemberRefParentToken(reference.Parent)),
                Column2 = _parentBuffer.StringStreamBuffer.GetStringOffset(reference.Name),
                Column3 = _parentBuffer.BlobStreamBuffer.GetBlobOffset(reference.Signature)
            };

            if (!_memberRefs.TryGetValue(memberRow, out token))
            {
                var table = (MemberReferenceTable) _tableStream.GetTable(MetadataTokenType.MemberRef);
                table.Add(memberRow);
                token = memberRow.MetadataToken;
                _memberRefs.Add(memberRow, token);
                _members.Add(reference, token);
                
                AddCustomAttributes(reference);
            }

            return token;
        }

        public MetadataToken GetMemberRefParentToken(IMemberRefParent parent)
        {
            AssertIsImported(parent);
            
            var type = parent as ITypeDefOrRef;
            if (type != null)
                return GetTypeToken(type);

            var method = parent as MethodDefinition;
            if (method != null)
                return GetNewToken(method);

            var reference = parent as ModuleReference;
            if (reference != null)
                return GetModuleReferenceToken(reference);
            
            throw new NotSupportedException("Invalid or unsupported MemberRefParent reference " + parent + ".");
        }

        public MetadataToken GetModuleReferenceToken(ModuleReference reference)
        {
            MetadataToken token;
            if (_members.TryGetValue(reference, out token))
                return token;
            
            AssertIsImported(reference);

            var referenceRow = new MetadataRow<uint>
            {
                Column1 = _parentBuffer.StringStreamBuffer.GetStringOffset(reference.Name)
            };

            if (!_moduleRefs.TryGetValue(referenceRow, out token))
            {
                var table = (ModuleReferenceTable) _tableStream.GetTable(MetadataTokenType.ModuleRef);
                table.Add(referenceRow);
                token = referenceRow.MetadataToken;
                _moduleRefs.Add(referenceRow, token);
                _members.Add(reference, token);

                AddCustomAttributes(reference);
            }

            return token;
        }

        public MetadataToken GetImplementationToken(IImplementation implementation)
        {
            // TODO
            throw new NotImplementedException();
        }

        public MetadataToken GetMethodSpecificationToken(MethodSpecification specification)
        {
            MetadataToken token;
            if (_members.TryGetValue(specification, out token))
                return token;

            AssertIsImported(specification);

            var specificationRow = new MetadataRow<uint, uint>
            {
                Column1 = _tableStream.GetIndexEncoder(CodedIndex.MethodDefOrRef)
                    .EncodeToken(GetMethodToken(specification.Method)),
                Column2 = _parentBuffer.BlobStreamBuffer.GetBlobOffset(specification.Signature)
            };

            if (!_methodSpecs.TryGetValue(specificationRow, out token))
            {
                var table = (MethodSpecificationTable) _tableStream.GetTable(MetadataTokenType.MethodSpec);
                table.Add(specificationRow);
                token = specificationRow.MetadataToken;
                _methodSpecs.Add(specificationRow, token);
                _members.Add(specification, token);
            }

            return token;
        }
        
        public void AddAssembly(AssemblyDefinition assembly)
        {
            var assemblyTable = (AssemblyDefinitionTable)_tableStream.GetTable(MetadataTokenType.Assembly);
            
            // Create and add assembly row.
            var assemblyRow = new MetadataRow<AssemblyHashAlgorithm, ushort, ushort, ushort, ushort, AssemblyAttributes, uint, uint, uint>
            {
                Column1 = assembly.HashAlgorithm,
                Column2 = (ushort) assembly.Version.Major,
                Column3 = (ushort) assembly.Version.Minor,
                Column4 = (ushort) assembly.Version.Build,
                Column5 = (ushort) assembly.Version.Revision,
                Column6 = assembly.Attributes,
                Column7 = _parentBuffer.BlobStreamBuffer.GetBlobOffset(assembly.PublicKey),
                Column8 = _parentBuffer.StringStreamBuffer.GetStringOffset(assembly.Name),
                Column9 = _parentBuffer.StringStreamBuffer.GetStringOffset(assembly.Culture == "neutral" ? null : assembly.Culture)
            };
            assemblyTable.Add(assemblyRow);
            _members.Add(assembly, assemblyRow.MetadataToken);
            
            // Add main module.
            AddModule(assembly.Modules[0]);

            // Add resources.
            foreach (var resource in assembly.Resources)
                AddManifestResource(resource);
            
            AddCustomAttributes(assembly);
            AddSecurityDeclarations(assembly);

            // TODO: add files, os and processors
        }

        private void AddModule(ModuleDefinition module)
        {
            var moduleTable = (ModuleDefinitionTable) _tableStream.GetTable(MetadataTokenType.Module);
            
            // Create and add module row.
            var moduleRow = new MetadataRow<ushort, uint, uint, uint, uint>
            {
                Column1 = module.Generation,
                Column2 = _parentBuffer.StringStreamBuffer.GetStringOffset(module.Name),
                Column3 = _parentBuffer.GuidStreamBuffer.GetGuidOffset(module.Mvid),
                Column4 = _parentBuffer.GuidStreamBuffer.GetGuidOffset(module.EncId),
                Column5 = _parentBuffer.GuidStreamBuffer.GetGuidOffset(module.EncBaseId)
            };
            moduleTable.Add(moduleRow);
            _members.Add(module, moduleRow.MetadataToken);

            // Add children.
            AddTypes(module.GetAllTypes().ToArray());
            AddCustomAttributes(module);
        }

        private void AddTypes(IList<TypeDefinition> types)
        {
            var typeTable = (TypeDefinitionTable) _tableStream.GetTable(MetadataTokenType.TypeDef);
            var fieldTable = (FieldDefinitionTable) _tableStream.GetTable(MetadataTokenType.Field);
            var methodTable = (MethodDefinitionTable) _tableStream.GetTable(MetadataTokenType.Method);
            
            // First create and add dummy rows without references, enforcing metadata tokens to be assigned.
            // This is required because typedefs can derive and/or reference each other.
            var typeRows = new List<MetadataRow<TypeAttributes, uint, uint, uint, uint, uint>>();

            foreach (var type in types)
            {
                AddDummyType(typeTable, fieldTable, methodTable, type);
                typeRows.Add(typeTable[typeTable.Count - 1]);
            }

            // Process created type rows.
            for (int i = 0; i < types.Count; i++)
                FinalizeTypeRow(types[i], typeRows[i], fieldTable, methodTable);
        }

        private void AddDummyType(TypeDefinitionTable typeTable, FieldDefinitionTable fieldTable, MethodDefinitionTable methodTable, TypeDefinition type)
        {
            // Add dummy type.
            var typeRow = new MetadataRow<TypeAttributes, uint, uint, uint, uint, uint>
            {
                Column1 = type.Attributes,
                Column2 = _parentBuffer.StringStreamBuffer.GetStringOffset(type.Name),
                Column3 = _parentBuffer.StringStreamBuffer.GetStringOffset(type.Namespace),
                Column4 = 0, // BaseType
                Column5 = _fieldList,
                Column6 = _methodList
            };
            typeTable.Add(typeRow);
            _members.Add(type, typeRow.MetadataToken);

            // Add dummy fields.
            foreach (var field in type.Fields)
                AddDummyField(fieldTable, field);

            _fieldList += (uint) type.Fields.Count;

            // Add dummy methods.
            foreach (var method in type.Methods)
                AddDummyMethod(methodTable, method);

            _methodList += (uint) type.Methods.Count;
        }

        private void AddDummyField(FieldDefinitionTable fieldTable, FieldDefinition field)
        {
            var fieldRow = new MetadataRow<FieldAttributes, uint, uint>
            {
                Column1 = field.Attributes,
                Column2 = _parentBuffer.StringStreamBuffer.GetStringOffset(field.Name),
                Column3 = 0 // Signature.
            };
            fieldTable.Add(fieldRow);
            _members.Add(field, fieldRow.MetadataToken);
        }

        private void AddDummyMethod(MethodDefinitionTable methodTable, MethodDefinition method)
        {
            var methodRow = new MetadataRow<FileSegment, MethodImplAttributes, MethodAttributes, uint, uint, uint>
            {
                Column1 = null, // Body
                Column2 = method.ImplAttributes,
                Column3 = method.Attributes,
                Column4 = _parentBuffer.StringStreamBuffer.GetStringOffset(method.Name),
                Column5 = 0, // Signature
                Column6 = _paramList
            };
            methodTable.Add(methodRow);
            _members.Add(method, methodRow.MetadataToken);
            _paramList += (uint) method.Parameters.Count;
        }

        private void FinalizeTypeRow(TypeDefinition type, MetadataRow<TypeAttributes, uint, uint, uint, uint, uint> typeRow, FieldDefinitionTable fieldTable,
            MethodDefinitionTable methodTable)
        {
            // Add base type.
            if (type.BaseType != null)
            {
                var baseToken = GetTypeToken(type.BaseType);
                typeRow.Column4 = _tableStream.GetIndexEncoder(CodedIndex.TypeDefOrRef).EncodeToken(baseToken);
            }

            // Finalize fields.
            foreach (var field in type.Fields)
            {
                var fieldRow = fieldTable[(int) (GetNewToken(field).Rid - 1)];
                FinalizeFieldRow(field, fieldRow);
            }

            // Finalize methods.
            foreach (var method in type.Methods)
            {
                var methodRow = methodTable[(int) (GetNewToken(method).Rid - 1)];
                FinalizeMethodRow(method, methodRow);
            }

            // Add nested classes.
            foreach (var nestedClass in type.NestedClasses)
                AddNestedClass(nestedClass);
            
            // Add interfaces.
            foreach (var @interface in type.Interfaces)
                AddInterface(@interface);

            // Add method implementations.
            foreach (var implementation in type.MethodImplementations)
                AddImplementation(implementation);

            // Add class layout if present.
            if (type.ClassLayout != null)
                AddClassLayout(type.ClassLayout);

            // Add property map if present.
            if (type.PropertyMap != null)
                AddPropertyMap(type.PropertyMap);

            // Add event map if present.
            if (type.EventMap != null)
                AddEventMap(type.EventMap);
            
            AddGenericParmeters(type);
            AddCustomAttributes(type);
            AddSecurityDeclarations(type);
        }

        private void FinalizeFieldRow(FieldDefinition field, MetadataRow<FieldAttributes, uint, uint> fieldRow)
        {
            // Update final column.
            fieldRow.Column3 = _parentBuffer.BlobStreamBuffer.GetBlobOffset(field.Signature);

            // Add optional extensions to field.
            if (field.Constant != null)
                AddConstant(field.Constant);
            
            if (field.FieldLayout != null)
                AddFieldLayout(field.FieldLayout);

            if (field.FieldMarshal != null)
                AddFieldMarshal(field.FieldMarshal);

            if (field.FieldRva != null)
                AddFieldRva(field.FieldRva);

            if (field.PInvokeMap != null)
                AddImplementationMap(field.PInvokeMap);
            
            AddCustomAttributes(field);
        }

        private void FinalizeMethodRow(MethodDefinition method,
            MetadataRow<FileSegment, MethodImplAttributes, MethodAttributes, uint, uint, uint> methodRow)
        {
            // Update remaining columns.
            if (method.MethodBody != null)
                methodRow.Column1 = method.MethodBody.CreateRawMethodBody(_parentBuffer);
            methodRow.Column5 = _parentBuffer.BlobStreamBuffer.GetBlobOffset(method.Signature);

            // Add parameters.
            foreach (var parameter in method.Parameters)
                AddParameter(parameter);

            if (method.PInvokeMap != null)
                AddImplementationMap(method.PInvokeMap);
            
            // Add optional extensions.
            AddGenericParmeters(method);
            AddCustomAttributes(method);
            AddSecurityDeclarations(method);
        }

        private void AddFieldLayout(FieldLayout fieldLayout)
        {
            var table = (FieldLayoutTable) _tableStream.GetTable(MetadataTokenType.FieldLayout);
            
            // Create and add row.
            var layoutRow = new MetadataRow<uint, uint>
            {
                Column1 = fieldLayout.Offset,
                Column2 = GetNewToken(fieldLayout.Field).Rid
            };
            table.Add(layoutRow);
            _members.Add(fieldLayout, layoutRow.MetadataToken);
        }

        private void AddFieldMarshal(FieldMarshal fieldMarshal)
        {
            var table = (FieldMarshalTable) _tableStream.GetTable(MetadataTokenType.FieldMarshal);
            
            // Create and add row.
            var marshalRow = new MetadataRow<uint, uint>
            {
                Column1 = _tableStream.GetIndexEncoder(CodedIndex.HasFieldMarshal).EncodeToken(GetNewToken(fieldMarshal.Parent)),
                Column2 = _parentBuffer.BlobStreamBuffer.GetBlobOffset(fieldMarshal.MarshalDescriptor)
            };
            table.Add(marshalRow);
            _members.Add(fieldMarshal, marshalRow.MetadataToken);
        }

        private void AddFieldRva(FieldRva fieldRva)
        {
            var table = (FieldRvaTable) _tableStream.GetTable(MetadataTokenType.FieldRva);
            
            // Create and add row.
            var rvaRow = new MetadataRow<DataSegment, uint>
            {
                Column1 = new DataSegment(fieldRva.Data),
                Column2 = GetNewToken(fieldRva.Field).Rid
            };
            table.Add(rvaRow);
            _members.Add(fieldRva, rvaRow.MetadataToken);
        }

        private void AddImplementationMap(ImplementationMap implementationMap)
        {
            var table = (ImplementationMapTable) _tableStream.GetTable(MetadataTokenType.ImplMap);
            
            // Create and add row.
            var mapRow = new MetadataRow<ImplementationMapAttributes, uint, uint, uint>
            {
                Column1 = implementationMap.Attributes,
                Column2 = _tableStream.GetIndexEncoder(CodedIndex.MemberForwarded)
                    .EncodeToken(GetNewToken(implementationMap.MemberForwarded)),
                Column3 = _parentBuffer.StringStreamBuffer.GetStringOffset(implementationMap.ImportName),
                Column4 = GetModuleReferenceToken(implementationMap.ImportScope).Rid
            };
            table.Add(mapRow);
            _members.Add(implementationMap, mapRow.MetadataToken);   
        }

        private void AddParameter(ParameterDefinition parameter)
        {
            var table = (ParameterDefinitionTable) _tableStream.GetTable(MetadataTokenType.Param);
            
            // Create and add row.
            var parameterRow = new MetadataRow<ParameterAttributes, ushort, uint>
            {
                Column1 = parameter.Attributes,
                Column2 = (ushort) parameter.Sequence,
                Column3 = _parentBuffer.StringStreamBuffer.GetStringOffset(parameter.Name)
            };
            table.Add(parameterRow);
            _members.Add(parameter, parameterRow.MetadataToken);

            // Add field marshal if present.
            if (parameter.FieldMarshal != null)
                AddFieldMarshal(parameter.FieldMarshal);
            
            AddCustomAttributes(parameter);
        }

        private void AddInterface(InterfaceImplementation @interface)
        {
            var table = (InterfaceImplementationTable) _tableStream.GetTable(MetadataTokenType.InterfaceImpl);
            
            // Create and add row.
            var interfaceRow = new MetadataRow<uint, uint>
            {
                Column1 = GetNewToken(@interface.Class).Rid,
                Column2 = _tableStream.GetIndexEncoder(CodedIndex.TypeDefOrRef).EncodeToken(GetTypeToken(@interface.Interface))
            };
            table.Add(interfaceRow);
            _members.Add(@interface, interfaceRow.MetadataToken);
            
            AddCustomAttributes(@interface);
        }

        private void AddImplementation(MethodImplementation implementation)
        {
            var table = (MethodImplementationTable) _tableStream.GetTable(MetadataTokenType.MethodImpl);
            
            // Create and add row.
            var encoder = _tableStream.GetIndexEncoder(CodedIndex.MethodDefOrRef);
            var implementationRow = new MetadataRow<uint, uint, uint>
            {
                Column1 = GetNewToken(implementation.Class).Rid,
                Column2 = encoder.EncodeToken(GetMethodToken(implementation.MethodBody)),
                Column3 = encoder.EncodeToken(GetMethodToken(implementation.MethodDeclaration))
            };
            table.Add(implementationRow);
            _members.Add(implementation, implementationRow.MetadataToken);
        }

        private void AddClassLayout(ClassLayout classLayout)
        {
            var table = (ClassLayoutTable) _tableStream.GetTable(MetadataTokenType.ClassLayout);
            
            // Create and add row.
            var layoutRow = new MetadataRow<ushort, uint, uint>
            {
                Column1 = classLayout.PackingSize,
                Column2 = classLayout.ClassSize,
                Column3 = GetNewToken(classLayout.Parent).Rid
            };
            table.Add(layoutRow);
            _members.Add(classLayout, layoutRow.MetadataToken);
        }

        private void AddPropertyMap(PropertyMap propertyMap)
        {
            var table = (PropertyMapTable) _tableStream.GetTable(MetadataTokenType.PropertyMap);
            
            // Create and add row.
            var mapRow = new MetadataRow<uint, uint>
            {
                Column1 = GetNewToken(propertyMap.Parent).Rid,
                Column2 = _propertyList
            };
            table.Add(mapRow);
            _members.Add(propertyMap, mapRow.MetadataToken);

            // Update property list for next property map.
            _propertyList += (uint) propertyMap.Properties.Count;

            // Add properties.
            foreach (var property in propertyMap.Properties)
                AddProperty(property);
        }

        private void AddProperty(PropertyDefinition property)
        {
            var table = (PropertyDefinitionTable) _tableStream.GetTable(MetadataTokenType.Property);
            
            // Create and add row.
            var propertyRow = new MetadataRow<PropertyAttributes, uint, uint>
            {
                Column1 = property.Attributes,
                Column2 = _parentBuffer.StringStreamBuffer.GetStringOffset(property.Name),
                Column3 = _parentBuffer.BlobStreamBuffer.GetBlobOffset(property.Signature)
            };
            table.Add(propertyRow);
            _members.Add(property, propertyRow.MetadataToken);

            // Add associated methods.
            foreach (var semantics in property.Semantics)
                AddSemantics(semantics);

            // Add constant if present.
            if (property.Constant != null)
                AddConstant(property.Constant);
            
            AddCustomAttributes(property);
        }

        private void AddEventMap(EventMap eventMap)
        {
            var table = (EventMapTable) _tableStream.GetTable(MetadataTokenType.EventMap);
            var mapRow = new MetadataRow<uint, uint>
            {
                Column1 = GetNewToken(eventMap.Parent).Rid,
                Column2 = _eventList
            };
            table.Add(mapRow);
            _members.Add(eventMap, mapRow.MetadataToken);

            _eventList += (uint) eventMap.Events.Count;

            foreach (var @event in eventMap.Events)
                AddEvent(@event);
        }

        private void AddEvent(EventDefinition @event)
        {
            var table = (EventDefinitionTable) _tableStream.GetTable(MetadataTokenType.Event);
            
            // Create and add row.
            var eventRow = new MetadataRow<EventAttributes, uint, uint>
            {
                Column1 = @event.Attributes,
                Column2 = _parentBuffer.StringStreamBuffer.GetStringOffset(@event.Name),
                Column3 = _tableStream.GetIndexEncoder(CodedIndex.TypeDefOrRef).EncodeToken(GetTypeToken(@event.EventType)),
            };
            table.Add(eventRow);
            _members.Add(@event, eventRow.MetadataToken);

            // Add associated methods.
            foreach (var semantic in @event.Semantics)
                AddSemantics(semantic);

            AddCustomAttributes(@event);
        }

        private void AddSemantics(MethodSemantics semantics)
        {
            var table = (MethodSemanticsTable) _tableStream.GetTable(MetadataTokenType.MethodSemantics);
            
            // Create and add row.
            var semanticsRow = new MetadataRow<MethodSemanticsAttributes, uint, uint>
            {
                Column1 = semantics.Attributes,
                Column2 = GetNewToken(semantics.Method).Rid,
                Column3 = _tableStream.GetIndexEncoder(CodedIndex.HasSemantics)
                    .EncodeToken(GetNewToken(semantics.Association))
            };
            table.Add(semanticsRow);
            _members.Add(semantics, semanticsRow.MetadataToken);
        }

        private void AddConstant(Constant constant)
        {
            var table = (ConstantTable) _tableStream.GetTable(MetadataTokenType.Constant);
            
            // Create and add row.
            var constantRow = new MetadataRow<ElementType, byte, uint, uint>
            {
                Column1 = constant.ConstantType,
                Column3 = _tableStream.GetIndexEncoder(CodedIndex.HasConstant)
                    .EncodeToken(GetNewToken(constant.Parent)),
                Column4 = _parentBuffer.BlobStreamBuffer.GetBlobOffset(constant.Value)
            };
            table.Add(constantRow);
            _members.Add(constant, constantRow.MetadataToken);
        }

        private void AddNestedClass(NestedClass nestedClass)
        {
            var table = (NestedClassTable) _tableStream.GetTable(MetadataTokenType.NestedClass);
            var classRow = new MetadataRow<uint, uint>
            {
                Column1 = GetNewToken(nestedClass.Class).Rid,
                Column2 = GetNewToken(nestedClass.EnclosingClass).Rid
            };
            table.Add(classRow);
            _members.Add(nestedClass, classRow.MetadataToken);
        }

        private void AddGenericParmeters(IGenericParameterProvider provider)
        {
            foreach (var parameter in provider.GenericParameters)
                AddGenericParmeter(parameter);
        }

        private void AddGenericParmeter(GenericParameter parameter)
        {
            var table = (GenericParameterTable) _tableStream.GetTable(MetadataTokenType.GenericParam);
            
            // Create and add row.
            var parameterRow = new MetadataRow<ushort, GenericParameterAttributes, uint, uint>
            {
                Column1 = (ushort) parameter.Index,
                Column2 = parameter.Attributes,
                Column3 = _tableStream.GetIndexEncoder(CodedIndex.TypeOrMethodDef).EncodeToken(GetNewToken(parameter.Owner)),
                Column4 = _parentBuffer.StringStreamBuffer.GetStringOffset(parameter.Name),
            };
            table.Add(parameterRow);
            _members.Add(parameter, parameterRow.MetadataToken);
            
            // Add parameter constraints
            foreach (var constraint in parameter.Constraints)
                AddGenericParameterConstraint(constraint);
        }

        private void AddGenericParameterConstraint(GenericParameterConstraint constraint)
        {
            var table = (GenericParameterConstraintTable) _tableStream.GetTable(MetadataTokenType.GenericParamConstraint);
            
            // Create and add row.
            var constraintRow = new MetadataRow<uint, uint>
            {
                Column1 = GetNewToken(constraint.Owner).Rid,
                Column2 = _tableStream.GetIndexEncoder(CodedIndex.TypeDefOrRef)
                    .EncodeToken(GetTypeToken(constraint.Constraint))
            };
            table.Add(constraintRow);
            _members.Add(constraint, constraintRow.MetadataToken);
        }

        private void AddManifestResource(ManifestResource resource)
        {
            var table = (ManifestResourceTable) _tableStream.GetTable(MetadataTokenType.ManifestResource);
            var resourceRow = new MetadataRow<uint, ManifestResourceAttributes, uint, uint>
            {
                Column1 = _parentBuffer.ResourcesBuffer.GetResourceOffset(resource), 
                Column2 = resource.Attributes,
                Column3 = _parentBuffer.StringStreamBuffer.GetStringOffset(resource.Name),
                Column4 = !resource.IsEmbedded
                    ? _tableStream.GetIndexEncoder(CodedIndex.Implementation)
                        .EncodeToken(GetImplementationToken(resource.Implementation))
                    : 0,
            };
            table.Add(resourceRow);
            _members.Add(resource, resourceRow.MetadataToken);
            
            AddCustomAttributes(resource);
        }

        public MetadataToken GetStandaloneSignatureToken(StandAloneSignature signature)
        {
            if (signature == null)
                return MetadataToken.Zero;

            MetadataToken token;
            if (_members.TryGetValue(signature, out token))
                return token;
            
            var table = (StandAloneSignatureTable) _tableStream.GetTable(MetadataTokenType.StandAloneSig);
            var signatureRow = new MetadataRow<uint>
            {
                Column1 = _parentBuffer.BlobStreamBuffer.GetBlobOffset(signature.Signature)
            };
            table.Add(signatureRow);
            _members.Add(signature, token = signatureRow.MetadataToken);

            AddCustomAttributes(signature);

            return token;
        }
        
        private void AddCustomAttributes(IHasCustomAttribute provider)
        {
            foreach (var attribute in provider.CustomAttributes)
                AddCustomAttribute(attribute);
        }

        private void AddSecurityDeclarations(IHasSecurityAttribute provider)
        {
            foreach (var attribute in provider.SecurityDeclarations)
                AddSecurityDeclaration(attribute);
        }

        private void AddSecurityDeclaration(SecurityDeclaration declaration)
        {
            var table = (SecurityDeclarationTable) _tableStream.GetTable(MetadataTokenType.DeclSecurity);
            
            // Create and add row.
            var declarationRow = new MetadataRow<SecurityAction, uint, uint>
            {
                Column1 = declaration.Action,
                Column2 = _tableStream.GetIndexEncoder(CodedIndex.HasDeclSecurity).EncodeToken(GetNewToken(declaration.Parent)),
                Column3 = _parentBuffer.BlobStreamBuffer.GetBlobOffset(declaration.PermissionSet)
            };
            table.Add(declarationRow);
            _members.Add(declaration, declarationRow.MetadataToken);
            
            AddCustomAttributes(declaration);
        }

        private void AddCustomAttribute(CustomAttribute attribute)
        {
            var table = (CustomAttributeTable) _tableStream.GetTable(MetadataTokenType.CustomAttribute);
            
            // Create and add row.
            var attributeRow = new MetadataRow<uint, uint, uint>
            {
                Column1 = _tableStream.GetIndexEncoder(CodedIndex.HasCustomAttribute).EncodeToken(GetNewToken(attribute.Parent)),
                Column2 = _tableStream.GetIndexEncoder(CodedIndex.CustomAttributeType).EncodeToken(GetMethodToken(attribute.Constructor)),
                Column3 = _parentBuffer.BlobStreamBuffer.GetBlobOffset(attribute.Signature)
            };
            table.Add(attributeRow);
            _members.Add(attribute, attributeRow.MetadataToken);
        }

        public override MetadataStream CreateStream()
        {
            _tableStream.BlobIndexSize = _parentBuffer.BlobStreamBuffer.Length > 0xFFFF ? IndexSize.Long : IndexSize.Short;
            _tableStream.StringIndexSize = _parentBuffer.StringStreamBuffer.Length > 0xFFFF ? IndexSize.Long : IndexSize.Short;
            _tableStream.GuidIndexSize = _parentBuffer.GuidStreamBuffer.Length > 0xFFFF ? IndexSize.Long : IndexSize.Short;
            _tableStream.ValidBitVector = _tableStream.ComputeValidBitVector();

            return _tableStream;
        }
    }
}