using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Emit
{
    /// <summary>
    /// Represents a buffer for constructing a new table stream.
    /// </summary>
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
        
        private readonly IDictionary<IMetadataMember, MetadataToken> _members = new Dictionary<IMetadataMember, MetadataToken>();
        
        private readonly IDictionary<TypeReference, MetadataToken> _typeRefs;
        private readonly IDictionary<TypeSpecification, MetadataToken> _typeSpecs;
        private readonly IDictionary<MetadataRow<ushort, ushort, ushort, ushort, AssemblyAttributes, uint, uint, uint, uint>, MetadataToken> _assemblyRefs;
        private readonly IDictionary<MetadataRow<uint>, MetadataToken> _moduleRefs;        
        private readonly IDictionary<MemberReference, MetadataToken> _memberRefs;
        private readonly IDictionary<MetadataRow<uint, uint>, MetadataToken> _methodSpecs;

        private readonly TableStream _tableStream;
        private readonly IList<Action> _fixups = new List<Action>();

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
            var sigComparer = new SignatureComparer();
            _typeRefs = new Dictionary<TypeReference, MetadataToken>(sigComparer);
            _typeSpecs = new Dictionary<TypeSpecification, MetadataToken>(sigComparer);
            _assemblyRefs = new Dictionary<MetadataRow<ushort, ushort, ushort, ushort, AssemblyAttributes, uint, uint, uint, uint>, MetadataToken>(rowComparer);
            _moduleRefs = new Dictionary<MetadataRow<uint>, MetadataToken>(rowComparer);
            _memberRefs = new Dictionary<MemberReference, MetadataToken>(sigComparer);
            _methodSpecs = new Dictionary<MetadataRow<uint, uint>, MetadataToken>(rowComparer);
        }

        public override string Name => "#~";

        public override uint Length => 0;

        /// <summary>
        /// Gets a dictionary that maps members to their new metadata tokens.
        /// </summary>
        /// <returns></returns>
        public IDictionary<IMetadataMember, MetadataToken> GetNewTokenMapping()
        {
            return _members;
        }
        
        /// <summary>
        /// Asserts whether a member is imported into the metadata image.
        /// </summary>
        /// <param name="member">The member to assert.</param>
        /// <exception cref="MemberNotImportedException">Occurs when the member is not imported.</exception>
        private void AssertIsImported(IMetadataMember member)
        {
            if (member.Image != _parentBuffer.Image)
                throw new MemberNotImportedException(member);
        }

        /// <summary>
        /// Gets the index encoder used for encoding metadata tokens that correspond to the provided coded index type. 
        /// </summary>
        /// <param name="codedIndex">The coded index to encode.</param>
        /// <returns>The encoder.</returns>
        public IndexEncoder GetIndexEncoder(CodedIndex codedIndex)
        {
            return _tableStream.GetIndexEncoder(codedIndex);
        }

        /// <summary>
        /// Gets the new metadata token for the provided member.
        /// </summary>
        /// <param name="member">The member to get the new metadata token from.</param>
        /// <returns>The new metadata token assigned to the member.</returns>
        /// <exception cref="MemberNotImportedException">Occurs when the member is not imported or added to the
        /// table stream.</exception>
        public MetadataToken GetNewToken(IMetadataMember member)
        {
            if (!_members.TryGetValue(member, out var token))
                throw new MemberNotImportedException(member);
            return token;
        }

        /// <summary>
        /// Gets the metadata token of a type. References to types that are not added to the table stream are added
        /// to the buffer. 
        /// </summary>
        /// <param name="type">The type to get the new metadata token from.</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public MetadataToken GetTypeToken(ITypeDefOrRef type)
        {
            switch (type)
            {
                case TypeDefinition definition:
                    return GetNewToken(definition);
                
                case TypeReference reference:
                    return GetTypeReferenceToken(reference);
                
                case TypeSpecification specification:
                    return GetTypeSpecificationToken(specification);
         
                default:
                    throw new NotSupportedException("Invalid or unsupported TypeDefOrRef reference " + type + ".");
            }
        }

        /// <summary>
        /// Gets the new metadata token of a type reference. Adds the reference to the buffer if it is not added yet.
        /// </summary>
        /// <param name="reference">The reference to get the token for.</param>
        /// <returns>The new metadata token assigned to the type reference.</returns>
        private MetadataToken GetTypeReferenceToken(TypeReference reference)
        {
            if (_members.TryGetValue(reference, out var token))
                return token;
            
            if (!_typeRefs.TryGetValue(reference, out token))
            {
                // Type ref is not added yet, check if imported and build new metadata row.
                AssertIsImported(reference);
                var typeRefRow = new MetadataRow<uint, uint, uint>
                {
                    Column1 = _tableStream.GetIndexEncoder(CodedIndex.ResolutionScope)
                        .EncodeToken(GetResolutionScopeToken(reference.ResolutionScope)),
                    Column2 = _parentBuffer.StringStreamBuffer.GetStringOffset(reference.Name),
                    Column3 = _parentBuffer.StringStreamBuffer.GetStringOffset(reference.Namespace),
                };
                
                var table = (TypeReferenceTable) _tableStream.GetTable(MetadataTokenType.TypeRef);
                table.Add(typeRefRow);
                
                // Register tokens.
                token = typeRefRow.MetadataToken;
                _typeRefs.Add(reference, token);
                _members.Add(reference, token);
                
                AddCustomAttributes(reference);
            }

            return token;
        }

        /// <summary>
        /// Gets the new metadata token of a type specification. Adds the specification to the buffer if it is not added yet.
        /// </summary>
        /// <param name="specification">The specification to get the token for.</param>
        /// <returns>The new metadata token assigned to the type specification.</returns>
        private MetadataToken GetTypeSpecificationToken(TypeSpecification specification)
        {
            if (_members.TryGetValue(specification, out var token))
                return token;

            if (!_typeSpecs.TryGetValue(specification, out token))
            {
                AssertIsImported(specification);

                // Type spec is not added yet, check if imported and build new metadata row.
                var typeSpecRow = new MetadataRow<uint>();
                specification.Signature.Prepare(_parentBuffer);
                _fixups.Add(() => typeSpecRow.Column1 = _parentBuffer.BlobStreamBuffer.GetBlobOffset(specification.Signature));
                
                var table = (TypeSpecificationTable) _tableStream.GetTable(MetadataTokenType.TypeSpec);
                table.Add(typeSpecRow);
                
                // Register tokens.
                token = typeSpecRow.MetadataToken;
                _typeSpecs.Add(specification, token);
                _members.Add(specification, token);
                
                AddCustomAttributes(specification);
            }

            return token;
        }

        /// <summary>
        /// Gets the metadata token of a resolution scope. References to scopes that are not added to the table stream
        /// are added to the buffer. 
        /// </summary>
        /// <param name="scope">The scope to get the new metadata token from.</param>
        /// <returns>The new metadata token assigned to the scope.</returns>
        /// <exception cref="NotSupportedException">Occurs when an unsupported resolution scope was provided.</exception>
        public MetadataToken GetResolutionScopeToken(IResolutionScope scope)
        {
            switch (scope)
            {
                case AssemblyReference assemblyRef:
                    return GetAssemblyReferenceToken(assemblyRef);
                
                case ModuleDefinition moduleDef:
                    return GetNewToken(moduleDef);
                
                case ModuleReference moduleRef:
                    return GetModuleReferenceToken(moduleRef);
                
                case TypeReference typeRef:
                    return GetTypeReferenceToken(typeRef);
             
                default:
                    throw new NotSupportedException("Invalid or unsupported ResolutionScope reference " + scope + ".");
            }
        }

        /// <summary>
        /// Gets the new metadata token of a reference to an assembly.
        /// Adds the reference to the buffer if it is not added yet.
        /// </summary>
        /// <param name="assemblyRef">The reference to get the token for.</param>
        /// <returns>The new metadata token assigned to the assembly reference.</returns>
        private MetadataToken GetAssemblyReferenceToken(AssemblyReference assemblyRef)
        {
            if (_members.TryGetValue(assemblyRef, out var token))
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
                    Column7 = _parentBuffer.StringStreamBuffer.GetStringOffset(assemblyRef.Name),
                    Column8 = _parentBuffer.StringStreamBuffer.GetStringOffset(assemblyRef.Culture == "neutral"
                        ? null
                        : assemblyRef.Culture),
                };

            if (!_assemblyRefs.TryGetValue(assemblyRow, out token))
            {
                assemblyRef.PublicKey?.Prepare(_parentBuffer);
                assemblyRef.HashValue?.Prepare(_parentBuffer);

                _fixups.Add(() =>
                {
                    assemblyRow.Column6 = _parentBuffer.BlobStreamBuffer.GetBlobOffset(assemblyRef.PublicKey);
                    assemblyRow.Column9 = _parentBuffer.BlobStreamBuffer.GetBlobOffset(assemblyRef.HashValue);
                });
                
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

        /// <summary>
        /// Gets the metadata token of a method. References to methods that are not added to the table stream
        /// are added to the buffer. 
        /// </summary>
        /// <param name="method">The method to get the new metadata token from.</param>
        /// <returns>The new metadata token assigned to the method.</returns>
        /// <exception cref="NotSupportedException">Occurs when an unsupported method was provided.</exception>
        public MetadataToken GetMethodToken(IMethodDefOrRef method)
        {
            switch (method)
            {
                case MethodDefinition definition:
                    return GetNewToken(definition);
                
                case MemberReference reference:
                    return GetMemberReferenceToken(reference);
                
                default:
                    throw new NotSupportedException($"Invalid or unsupported MethodDefOrRef reference + {method}.");
            }
        }

        /// <summary>
        /// Gets the metadata token of a reference to an external member. References that are not added to the table
        /// stream are added to the buffer.
        /// </summary>
        /// <param name="reference">The reference to get the new metadata token from.</param>
        /// <returns>The new metadata token assigned to the reference.</returns>
        public MetadataToken GetMemberReferenceToken(MemberReference reference)
        {
            if (_members.TryGetValue(reference, out var token))
                return token;

            if (!_memberRefs.TryGetValue(reference, out token))
            {
                // Reference is not added yet. Check if imported and build row.
                AssertIsImported(reference);
                var memberRow = new MetadataRow<uint, uint, uint>
                {
                    Column1 = _tableStream.GetIndexEncoder(CodedIndex.MemberRefParent).EncodeToken(GetMemberRefParentToken(reference.Parent)),
                    Column2 = _parentBuffer.StringStreamBuffer.GetStringOffset(reference.Name),
                };

                // Ensure declaring type is added as well.
                GetTypeToken(reference.DeclaringType);
            
                reference.Signature.Prepare(_parentBuffer);
                _fixups.Add(() => memberRow.Column3 = _parentBuffer.BlobStreamBuffer.GetBlobOffset(reference.Signature));
                
                var table = (MemberReferenceTable) _tableStream.GetTable(MetadataTokenType.MemberRef);
                table.Add(memberRow);
                
                // Register tokens.
                token = memberRow.MetadataToken;
                _memberRefs.Add(reference, token);
                _members.Add(reference, token);
                
                AddCustomAttributes(reference);
            }

            return token;
        }

        /// <summary>
        /// Gets the metadata token of a parent of a member reference. Parents that are not added to the table
        /// stream are added to the buffer.
        /// </summary>
        /// <param name="parent">The parent to get the new metadata token from.</param>
        /// <returns>The new metadata token assigned to the parent.</returns>
        public MetadataToken GetMemberRefParentToken(IMemberRefParent parent)
        {
            AssertIsImported(parent);

            switch (parent)
            {
                case ITypeDefOrRef type:
                    return GetTypeToken(type);
                
                case MethodDefinition method:
                    return GetNewToken(method);
                
                case ModuleReference reference:
                    return GetModuleReferenceToken(reference);
                
                default:
                    throw new NotSupportedException($"Invalid or unsupported MemberRefParent reference {parent}.");
            }
        }

        /// <summary>
        /// Gets the metadata token of a reference to an external module. References that are not added to the table
        /// stream are added to the buffer.
        /// </summary>
        /// <param name="reference">The reference to get the new metadata token from.</param>
        /// <returns>The new metadata token assigned to the reference.</returns>
        private MetadataToken GetModuleReferenceToken(ModuleReference reference)
        {
            if (_members.TryGetValue(reference, out var token))
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

        /// <summary>
        /// Gets the metadata token of a method specification. Specifications that are not added to the table
        /// stream are added to the buffer.
        /// </summary>
        /// <param name="specification">The specification to get the new metadata token from.</param>
        /// <returns>The new metadata token assigned to the specification.</returns>
        public MetadataToken GetMethodSpecificationToken(MethodSpecification specification)
        {
            if (_members.TryGetValue(specification, out var token))
                return token;

            AssertIsImported(specification);

            var specificationRow = new MetadataRow<uint, uint>
            {
                Column1 = _tableStream.GetIndexEncoder(CodedIndex.MethodDefOrRef)
                    .EncodeToken(GetMethodToken(specification.Method))
            };

            if (!_methodSpecs.TryGetValue(specificationRow, out token))
            {
                specification.Signature.Prepare(_parentBuffer);
                _fixups.Add(() =>
                    specificationRow.Column2 = _parentBuffer.BlobStreamBuffer.GetBlobOffset(specification.Signature));
                
                var table = (MethodSpecificationTable) _tableStream.GetTable(MetadataTokenType.MethodSpec);
                table.Add(specificationRow);
                token = specificationRow.MetadataToken;
                _methodSpecs.Add(specificationRow, token);
                _members.Add(specification, token);
            }

            return token;
        }
        
        /// <summary>
        /// Adds an entire assembly and all its components to the table stream buffer.
        /// </summary>
        /// <param name="assembly">The assembly to add.</param>
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
                Column8 = _parentBuffer.StringStreamBuffer.GetStringOffset(assembly.Name),
                Column9 = _parentBuffer.StringStreamBuffer.GetStringOffset(assembly.Culture == "neutral" ? null : assembly.Culture)
            };

            assembly.PublicKey?.Prepare(_parentBuffer);
            _fixups.Add(() => 
                assemblyRow.Column7 = _parentBuffer.BlobStreamBuffer.GetBlobOffset(assembly.PublicKey));
            
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

        /// <summary>
        /// Adds an entire module definition and all its components to the table stream buffer.
        /// </summary>
        /// <param name="module">The module to add.</param>
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

        /// <summary>
        /// Adds a collection of types and all its members to the table stream buffer.
        /// </summary>
        /// <param name="types"></param>
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
                AddTypeStub(typeTable, fieldTable, methodTable, type);
                typeRows.Add(typeTable[typeTable.Count - 1]);
            }

            // Process created type rows.
            for (int i = 0; i < types.Count; i++)
                FinalizeTypeRow(types[i], typeRows[i], fieldTable, methodTable);
        }

        /// <summary>
        /// Adds a stub metadata row for the provided type, as well as stub rows for the fields and methods defined in
        /// the type to the table buffer stream.
        /// </summary>
        /// <param name="typeTable">The table containing the type definition rows to add the type to.</param>
        /// <param name="fieldTable">The table containing the field definitions.</param>
        /// <param name="methodTable">The table containing the method definitions.</param>
        /// <param name="type">The type to add.</param>
        private void AddTypeStub(TypeDefinitionTable typeTable, FieldDefinitionTable fieldTable, MethodDefinitionTable methodTable, TypeDefinition type)
        {
            // Add dummy type.
            var typeRow = new MetadataRow<TypeAttributes, uint, uint, uint, uint, uint>
            {
                Column1 = type.Attributes,
                Column2 = _parentBuffer.StringStreamBuffer.GetStringOffset(type.Name),
                Column3 = _parentBuffer.StringStreamBuffer.GetStringOffset(type.Namespace),
                Column4 = 0, // BaseType, updated later.
                Column5 = _fieldList,
                Column6 = _methodList
            };
            typeTable.Add(typeRow);
            _members.Add(type, typeRow.MetadataToken);

            // Add dummy fields.
            foreach (var field in type.Fields)
                AddFieldStub(fieldTable, field);

            _fieldList += (uint) type.Fields.Count;

            // Add dummy methods.
            foreach (var method in type.Methods)
                AddMethodStub(methodTable, method);

            _methodList += (uint) type.Methods.Count;
        }

        /// <summary>
        /// Adds a stub metadata row for the provided field to the table stream buffer.
        /// </summary>
        /// <param name="fieldTable">The field table to add the metadata row to.</param>
        /// <param name="field">The field to add.</param>
        private void AddFieldStub(FieldDefinitionTable fieldTable, FieldDefinition field)
        {
            var fieldRow = new MetadataRow<FieldAttributes, uint, uint>
            {
                Column1 = field.Attributes,
                Column2 = _parentBuffer.StringStreamBuffer.GetStringOffset(field.Name),
                Column3 = 0 // Signature, updated later.
            };
            fieldTable.Add(fieldRow);
            _members.Add(field, fieldRow.MetadataToken);
        }

        /// <summary>
        /// Adds a stub metadata row for the provided method to the table stream buffer.
        /// </summary>
        /// <param name="methodTable">The method table to add the metadata row to.</param>
        /// <param name="method">The method to add.</param>
        private void AddMethodStub(MethodDefinitionTable methodTable, MethodDefinition method)
        {
            var methodRow = new MetadataRow<FileSegment, MethodImplAttributes, MethodAttributes, uint, uint, uint>
            {
                Column1 = null, // Body, updated later.
                Column2 = method.ImplAttributes,
                Column3 = method.Attributes,
                Column4 = _parentBuffer.StringStreamBuffer.GetStringOffset(method.Name),
                Column5 = 0, // Signature, updated later.
                Column6 = _paramList
            };
            methodTable.Add(methodRow);
            _members.Add(method, methodRow.MetadataToken);
            _paramList += (uint) method.Parameters.Count;
        }

        /// <summary>
        /// Finalizes a metadata row stub of a type definition, as well as all the metadata row stubs of the members
        /// defined by the type definition. 
        /// </summary>
        /// <param name="type">The type definition to finalize.</param>
        /// <param name="typeRow">The metadata row stub of the type definition referenced by <paramref name="type"/>.</param>
        /// <param name="fieldTable">The table containing all fields defined by the assembly.</param>
        /// <param name="methodTable">The table containing all methods defined by the assembly.</param>
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

        /// <summary>
        /// Finalizes a field metadata row stub.
        /// </summary>
        /// <param name="field">The field to finalize.</param>
        /// <param name="fieldRow">The metadata row stub associated to the field definition referenced by <paramref name="field"/>.</param>
        private void FinalizeFieldRow(FieldDefinition field, MetadataRow<FieldAttributes, uint, uint> fieldRow)
        {
            // Update final column.
            field.Signature?.Prepare(_parentBuffer);
            _fixups.Add(() => fieldRow.Column3 = _parentBuffer.BlobStreamBuffer.GetBlobOffset(field.Signature));

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

        /// <summary>
        /// Finalizes a method metadata row stub.
        /// </summary>
        /// <param name="method">The method to finalize.</param>
        /// <param name="methodRow">The metadata row stub associated to the method definition referenced by <paramref name="method"/>.</param>
        private void FinalizeMethodRow(MethodDefinition method,
            MetadataRow<FileSegment, MethodImplAttributes, MethodAttributes, uint, uint, uint> methodRow)
        {
            // Update remaining columns.
            if (method.MethodBody != null)
                methodRow.Column1 = method.MethodBody.CreateRawMethodBody(_parentBuffer);

            method.Signature?.Prepare(_parentBuffer);
            _fixups.Add(() => methodRow.Column5 = _parentBuffer.BlobStreamBuffer.GetBlobOffset(method.Signature));

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

        /// <summary>
        /// Adds a field layout metadata row to the table stream buffer.
        /// </summary>
        /// <param name="fieldLayout">The field layout to add.</param>
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

        /// <summary>
        /// Adds a field marshal metadata row to the table stream buffer.
        /// </summary>
        /// <param name="fieldMarshal">The field marshal to add.</param>
        /// <remarks>
        /// This method assumes the metadata row of the field in the provided field marshal is already added to the buffer.
        /// </remarks>
        private void AddFieldMarshal(FieldMarshal fieldMarshal)
        {
            var table = (FieldMarshalTable) _tableStream.GetTable(MetadataTokenType.FieldMarshal);
            
            // Create and add row.
            var marshalRow = new MetadataRow<uint, uint>
            {
                Column1 = _tableStream.GetIndexEncoder(CodedIndex.HasFieldMarshal).EncodeToken(GetNewToken(fieldMarshal.Parent)),
            };

            fieldMarshal.MarshalDescriptor?.Prepare(_parentBuffer);
            _fixups.Add(() =>
                marshalRow.Column2 = _parentBuffer.BlobStreamBuffer.GetBlobOffset(fieldMarshal.MarshalDescriptor));
            
            table.Add(marshalRow);
            _members.Add(fieldMarshal, marshalRow.MetadataToken);
        }

        /// <summary>
        /// Adds a field RVA metadata row to the table stream buffer.
        /// </summary>
        /// <param name="fieldRva"></param>
        /// <remarks>
        /// This method assumes the metadata row of the field in the provided field RVA is already added to the buffer.
        /// </remarks>
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

        /// <summary>
        /// Adds an implementation mapping metadata row to the table stream buffer.
        /// </summary>
        /// <param name="implementationMap">The mapping to add.</param>
        /// <remarks>
        /// This method assumes the metadata rows of the method and the module in the provided map is already added to
        /// the buffer.
        /// </remarks>
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

        /// <summary>
        /// Adds a parameter metadata row to the table stream buffer.
        /// </summary>
        /// <param name="parameter">The parameter to add.</param>
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

            // Add constant if present.
            if (parameter.Constant != null)
                AddConstant(parameter.Constant);
            
            AddCustomAttributes(parameter);
        }

        /// <summary>
        /// Adds an interface implementation to the table stream buffer.
        /// </summary>
        /// <param name="interface">The interface to add.</param>
        /// <remarks>
        /// This method assumes the metadata rows of the referenced type definitions are already added.
        /// </remarks>
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

        /// <summary>
        /// Adds a method implementation metadata row to the table stream buffer.
        /// </summary>
        /// <param name="implementation">The implementation to add.</param>
        /// <remarks>
        /// This method assumes the metadata rows of the referenced type definitions are already added.
        /// </remarks>
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

        /// <summary>
        /// Adds a class layout metadata row to the table stream buffer.
        /// </summary>
        /// <param name="classLayout">The class layout to add.</param>
        /// <remarks>
        /// This method assumes the metadata rows of the referenced type definitions are already added.
        /// </remarks>
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

        /// <summary>
        /// Adds a property map metadata row to the table stream buffer.
        /// </summary>
        /// <param name="propertyMap">The property map to add.</param>
        /// <remarks>
        /// This method assumes the metadata row of the parent type is already added.
        /// </remarks>
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

        /// <summary>
        /// Adds a property metadata row to the table stream buffer.
        /// </summary>
        /// <param name="property">The property to add.</param>
        /// <remarks>
        /// This method assumes the metadata row of the parent property map is already added.
        /// </remarks>
        private void AddProperty(PropertyDefinition property)
        {
            var table = (PropertyDefinitionTable) _tableStream.GetTable(MetadataTokenType.Property);
            
            // Create and add row.
            var propertyRow = new MetadataRow<PropertyAttributes, uint, uint>
            {
                Column1 = property.Attributes,
                Column2 = _parentBuffer.StringStreamBuffer.GetStringOffset(property.Name),
            };

            property.Signature.Prepare(_parentBuffer);
            _fixups.Add(() => propertyRow.Column3 = _parentBuffer.BlobStreamBuffer.GetBlobOffset(property.Signature));
            
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

        /// <summary>
        /// Adds a event map metadata row to the table stream buffer.
        /// </summary>
        /// <param name="eventMap">The event map to add.</param>
        /// <remarks>
        /// This method assumes the metadata rows of the parent property map and semantic methods are already added.
        /// </remarks>
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

        /// <summary>
        /// Adds a event metadata row to the table stream buffer.
        /// </summary>
        /// <param name="event">The event to add.</param>
        /// <remarks>
        /// This method assumes the metadata rows of the parent property map and semantic methods are already added.
        /// </remarks>
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

        /// <summary>
        /// Adds a method semantics metadata row to the table stream buffer.
        /// </summary>
        /// <param name="semantics">The semantics to add.</param>
        /// <remarks>
        /// This method assumes the metadata rows of the owner and method are already added.
        /// </remarks>
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

        /// <summary>
        /// Adds a constant metadata row to the table stream buffer.
        /// </summary>
        /// <param name="constant">The constant to add.</param>
        /// <remarks>
        /// This method assumes the metadata row of the field is already added.
        /// </remarks>
        private void AddConstant(Constant constant)
        {
            var table = (ConstantTable) _tableStream.GetTable(MetadataTokenType.Constant);
            
            // Create and add row.
            var constantRow = new MetadataRow<ElementType, byte, uint, uint>
            {
                Column1 = constant.ConstantType,
                Column3 = _tableStream.GetIndexEncoder(CodedIndex.HasConstant)
                    .EncodeToken(GetNewToken(constant.Parent)),
            };

            constant.Value?.Prepare(_parentBuffer);

            _fixups.Add(() => constantRow.Column4 = _parentBuffer.BlobStreamBuffer.GetBlobOffset(constant.Value));
                
            table.Add(constantRow);
            _members.Add(constant, constantRow.MetadataToken);
        }

        /// <summary>
        /// Adds a nested class metadata row to the table stream buffer.
        /// </summary>
        /// <param name="nestedClass">The nested class to add.</param>
        /// <remarks>
        /// This method assumes the metadata rows of the type definitions are already added.
        /// </remarks>
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

        /// <summary>
        /// Adds a collection of generic parameters to the table stream buffer.
        /// </summary>
        /// <param name="provider">The member containing the parameters to add.</param>
        private void AddGenericParmeters(IGenericParameterProvider provider)
        {
            foreach (var parameter in provider.GenericParameters)
                AddGenericParmeter(parameter);
        }

        /// <summary>
        /// Adds a generic parameter metadata row to the table stream buffer.
        /// </summary>
        /// <param name="parameter">The parameter to add.</param>
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

        /// <summary>
        /// Adds a generic parameter constraint metadata row to the table stream buffer.
        /// </summary>
        /// <param name="constraint">The parameter constraint to add.</param>
        /// <remarks>
        /// This method assumes the generic parameter is already added.
        /// </remarks>
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

        /// <summary>
        /// Adds a manifest resource metadata row to the table stream buffer.
        /// </summary>
        /// <param name="resource">The resource to add.</param>
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

        /// <summary>
        /// Gets the new metadata token of a standalone signature. Signatures that are not added to the table stream
        /// will be added to the buffer.
        /// </summary>
        /// <param name="signature">The signature to get the token for.</param>
        /// <returns>The new metadata token assigned to the signature.</returns>
        public MetadataToken GetStandaloneSignatureToken(StandAloneSignature signature)
        {
            if (signature == null)
                return MetadataToken.Zero;

            if (_members.TryGetValue(signature, out var token))
                return token;
            
            var table = (StandAloneSignatureTable) _tableStream.GetTable(MetadataTokenType.StandAloneSig);
            var signatureRow = new MetadataRow<uint>();

            signature.Signature?.Prepare(_parentBuffer);
            _fixups.Add(() => signatureRow.Column1 = _parentBuffer.BlobStreamBuffer.GetBlobOffset(signature.Signature));
            
            table.Add(signatureRow);
            _members.Add(signature, token = signatureRow.MetadataToken);

            AddCustomAttributes(signature);

            return token;
        }
        
        /// <summary>
        /// Adds a collection of custom attribute metadata rows to the table stream buffer.
        /// </summary>
        /// <param name="provider">The member containing the custom attributes to add.</param>
        private void AddCustomAttributes(IHasCustomAttribute provider)
        {
            foreach (var attribute in provider.CustomAttributes)
                AddCustomAttribute(attribute);
        }

        /// <summary>
        /// Adds a custom attribute metadata row to the table stream buffer.
        /// </summary>
        /// <param name="attribute">The attribute to add.</param>
        private void AddCustomAttribute(CustomAttribute attribute)
        {
            var table = (CustomAttributeTable) _tableStream.GetTable(MetadataTokenType.CustomAttribute);
            
            // Create and add row.
            var attributeRow = new MetadataRow<uint, uint, uint>
            {
                Column1 = _tableStream.GetIndexEncoder(CodedIndex.HasCustomAttribute).EncodeToken(GetNewToken(attribute.Parent)),
                Column2 = _tableStream.GetIndexEncoder(CodedIndex.CustomAttributeType).EncodeToken(GetMethodToken(attribute.Constructor)),
            };

            attribute.Signature?.Prepare(_parentBuffer);
            _fixups.Add(() => attributeRow.Column3 = _parentBuffer.BlobStreamBuffer.GetBlobOffset(attribute.Signature));
            
            table.Add(attributeRow);
            _members.Add(attribute, attributeRow.MetadataToken);
        }


        /// <summary>
        /// Adds a collection of security attribute metadata row to the table stream buffer.
        /// </summary>
        /// <param name="provider">The member containing the security attributes to add.</param>
        private void AddSecurityDeclarations(IHasSecurityAttribute provider)
        {
            foreach (var attribute in provider.SecurityDeclarations)
                AddSecurityDeclaration(attribute);
        }

        /// <summary>
        /// Adds a security attribute metadata row to the table stream buffer.
        /// </summary>
        /// <param name="declaration">The attribute to add.</param>
        private void AddSecurityDeclaration(SecurityDeclaration declaration)
        {
            var table = (SecurityDeclarationTable) _tableStream.GetTable(MetadataTokenType.DeclSecurity);
            
            // Create and add row.
            var declarationRow = new MetadataRow<SecurityAction, uint, uint>
            {
                Column1 = declaration.Action,
                Column2 = _tableStream.GetIndexEncoder(CodedIndex.HasDeclSecurity).EncodeToken(GetNewToken(declaration.Parent)),
            };

            declaration.PermissionSet?.Prepare(_parentBuffer);
            _fixups.Add(() => 
                declarationRow.Column3 = _parentBuffer.BlobStreamBuffer.GetBlobOffset(declaration.PermissionSet));
            
            table.Add(declarationRow);
            _members.Add(declaration, declarationRow.MetadataToken);
            
            AddCustomAttributes(declaration);
        }
        
        public override MetadataStream CreateStream()
        {
            for (var i = 0; i < _fixups.Count; i++) 
                _fixups[i]();

            _tableStream.BlobIndexSize = _parentBuffer.BlobStreamBuffer.Length > 0xFFFF ? IndexSize.Long : IndexSize.Short;
            _tableStream.StringIndexSize = _parentBuffer.StringStreamBuffer.Length > 0xFFFF ? IndexSize.Long : IndexSize.Short;
            _tableStream.GuidIndexSize = _parentBuffer.GuidStreamBuffer.Length > 0xFFFF ? IndexSize.Long : IndexSize.Short;
            _tableStream.ValidBitVector = _tableStream.ComputeValidBitVector();

            return _tableStream;
        }
    }
}