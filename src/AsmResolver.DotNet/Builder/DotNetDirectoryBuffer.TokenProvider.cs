using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.Builder
{
    public partial class DotNetDirectoryBuffer : IMetadataTokenProvider
    {
        /// <inheritdoc />
        public uint GetUserStringIndex(string value) => Metadata.UserStringsStream.GetStringIndex(value);

        /// <inheritdoc />
        public MetadataToken GetTypeReferenceToken(TypeReference? type, object? diagnosticSource = null)
        {
            return AddTypeReference(type, false, false, diagnosticSource);
        }

        /// <summary>
        /// Adds a type reference to the buffer.
        /// </summary>
        /// <param name="type">The reference to add.</param>
        /// <param name="allowDuplicates">
        /// <c>true</c> if the row is always to be added to the end of the buffer, <c>false</c> if a duplicated row
        /// is supposed to be removed and the token of the original should be returned instead.
        /// </param>
        /// <param name="preserveRid">
        /// <c>true</c> if the metadata token of the type should be preserved, <c>false</c> otherwise.
        /// </param>
        /// <param name="diagnosticSource">The object that referenced the type.</param>
        /// <returns>The newly assigned metadata token.</returns>
        public MetadataToken AddTypeReference(TypeReference? type, bool allowDuplicates, bool preserveRid, object? diagnosticSource = null)
        {
            if (!AssertIsImported(type, diagnosticSource))
                return MetadataToken.Zero;

            var table = Metadata.TablesStream.GetDistinctTable<TypeReferenceRow>(TableIndex.TypeRef);
            var row = new TypeReferenceRow(
                AddResolutionScope(type.Scope, allowDuplicates, preserveRid, type),
                Metadata.StringsStream.GetStringIndex(type.Name),
                Metadata.StringsStream.GetStringIndex(type.Namespace));

            var token = preserveRid && type.MetadataToken.Rid != 0
                ? table.Insert(type.MetadataToken.Rid, row, allowDuplicates)
                : table.Add(row, allowDuplicates);

            _tokenMapping.Register(type, token);
            AddCustomAttributes(token, type);
            return token;
        }

        /// <inheritdoc />
        public MetadataToken GetTypeDefinitionToken(TypeDefinition? type, object? diagnosticSource = null)
        {
            return AssertIsImported(type, diagnosticSource)
                ? _tokenMapping[type]
                : MetadataToken.Zero;
        }

        /// <inheritdoc />
        public MetadataToken GetFieldDefinitionToken(FieldDefinition? field, object? diagnosticSource = null)
        {
            return AssertIsImported(field, diagnosticSource)
                ? _tokenMapping[field]
                : MetadataToken.Zero;
        }

        /// <inheritdoc />
        public MetadataToken GetMethodDefinitionToken(MethodDefinition? method, object? diagnosticSource = null)
        {
            return AssertIsImported(method, diagnosticSource)
                ? _tokenMapping[method]
                : MetadataToken.Zero;
        }

        /// <summary>
        /// Gets the newly assigned metadata token of a parameter definition stored in a tables stream or tables
        /// stream buffer.
        /// </summary>
        /// <param name="parameter">The reference to the parameter to add.</param>
        /// <returns>The metadata token of the added parameter definition.</returns>
        public MetadataToken GetParameterDefinitionToken(ParameterDefinition? parameter)
        {
            return AssertIsImported(parameter, parameter?.Method)
                ? _tokenMapping[parameter]
                : MetadataToken.Zero;
        }

        /// <summary>
        /// Gets the newly assigned metadata token of a property definition stored in a tables stream or tables stream buffer.
        /// </summary>
        /// <param name="property">The reference to the property to add.</param>
        /// <returns>The metadata token of the added property definition.</returns>
        public MetadataToken GetPropertyDefinitionToken(PropertyDefinition? property)
        {
            return AssertIsImported(property, property?.DeclaringType)
                ? _tokenMapping[property]
                : MetadataToken.Zero;
        }

        /// <summary>
        /// Gets the newly assigned metadata token of an event definition stored in a tables stream or tables stream buffer.
        /// </summary>
        /// <param name="event">The reference to the event to add.</param>
        /// <returns>The metadata token of the added event definition.</returns>
        public MetadataToken GetEventDefinitionToken(EventDefinition? @event)
        {
            return AssertIsImported(@event, @event?.DeclaringType)
                ? _tokenMapping[@event]
                : MetadataToken.Zero;
        }

        /// <inheritdoc />
        public MetadataToken GetMemberReferenceToken(MemberReference? member, object? diagnosticSource = null)
        {
            return AddMemberReference(member, false, diagnosticSource);
        }

        /// <summary>
        /// Adds a member reference to the buffer.
        /// </summary>
        /// <param name="member">The reference to add.</param>
        /// <param name="allowDuplicates">
        /// <c>true</c> if the row is always to be added to the end of the buffer, <c>false</c> if a duplicated row
        /// is supposed to be removed and the token of the original should be returned instead.
        /// </param>
        /// <param name="diagnosticSource">The object that referenced the member.</param>
        /// <returns>The newly assigned metadata token.</returns>
        public MetadataToken AddMemberReference(MemberReference? member, bool allowDuplicates, object? diagnosticSource = null)
        {
            if (!AssertIsImported(member, diagnosticSource))
                return MetadataToken.Zero;

            var table = Metadata.TablesStream.GetDistinctTable<MemberReferenceRow>(TableIndex.MemberRef);
            var row = new MemberReferenceRow(
                AddMemberRefParent(member.Parent, member),
                Metadata.StringsStream.GetStringIndex(member.Name),
                Metadata.BlobStream.GetBlobIndex(this, member.Signature, ErrorListener, diagnosticSource));

            var token = table.Add(row, allowDuplicates);
            _tokenMapping.Register(member, token);
            AddCustomAttributes(token, member);
            return token;
        }

        /// <inheritdoc />
        public MetadataToken GetStandAloneSignatureToken(StandAloneSignature? signature, object? diagnosticSource = null)
        {
            return AddStandAloneSignature(signature, false, diagnosticSource);
        }

        /// <summary>
        /// Adds a stand-alone signature to the buffer.
        /// </summary>
        /// <param name="signature">The signature to add.</param>
        /// <param name="allowDuplicates">
        /// <c>true</c> if the row is always to be added to the end of the buffer, <c>false</c> if a duplicated row
        /// is supposed to be removed and the token of the original should be returned instead.
        /// </param>
        /// <param name="diagnosticSource">The object that referenced the standalone signature.</param>
        /// <returns>The newly assigned metadata token.</returns>
        public MetadataToken AddStandAloneSignature(StandAloneSignature? signature, bool allowDuplicates, object? diagnosticSource = null)
        {
            if (signature is null)
                return MetadataToken.Zero;

            var table = Metadata.TablesStream.GetDistinctTable<StandAloneSignatureRow>(TableIndex.StandAloneSig);
            var row = new StandAloneSignatureRow(
                Metadata.BlobStream.GetBlobIndex(this, signature.Signature, ErrorListener, diagnosticSource));

            var token = table.Add(row, allowDuplicates);
            _tokenMapping.Register(signature, token);
            AddCustomAttributes(token, signature);
            return token;
        }

        /// <inheritdoc />
        public MetadataToken GetAssemblyReferenceToken(AssemblyReference? assembly, object? diagnosticSource = null)
        {
            return AddAssemblyReference(assembly, false, false, diagnosticSource);
        }

        /// <summary>
        /// Adds an assembly reference to the buffer.
        /// </summary>
        /// <param name="assembly">The reference to add.</param>
        /// <param name="allowDuplicates">
        /// <c>true</c> if the row is always to be added to the end of the buffer, <c>false</c> if a duplicated row
        /// is supposed to be removed and the token of the original should be returned instead.
        /// </param>
        /// <param name="preserveRid">
        /// <c>true</c> if the metadata token of the assembly should be preserved, <c>false</c> otherwise.
        /// </param>
        /// <param name="diagnosticSource">The object that referenced the assembly.</param>
        /// <returns>The newly assigned metadata token.</returns>
        public MetadataToken AddAssemblyReference(AssemblyReference? assembly, bool allowDuplicates, bool preserveRid, object? diagnosticSource = null)
        {
            if (assembly is null || !AssertIsImported(assembly, diagnosticSource))
                return MetadataToken.Zero;

            var table = Metadata.TablesStream.GetDistinctTable<AssemblyReferenceRow>(TableIndex.AssemblyRef);

            var row = new AssemblyReferenceRow((ushort) assembly.Version.Major,
                (ushort) assembly.Version.Minor,
                (ushort) assembly.Version.Build,
                (ushort) assembly.Version.Revision,
                assembly.Attributes,
                Metadata.BlobStream.GetBlobIndex(assembly.PublicKeyOrToken),
                Metadata.StringsStream.GetStringIndex(assembly.Name),
                Metadata.StringsStream.GetStringIndex(assembly.Culture),
                Metadata.BlobStream.GetBlobIndex(assembly.HashValue));

            var token = preserveRid && assembly.MetadataToken.Rid != 0
                ? table.Insert(assembly.MetadataToken.Rid, row, allowDuplicates)
                : table.Add(row, allowDuplicates);

            AddCustomAttributes(token, assembly);
            return token;
        }

        /// <summary>
        /// Adds a single module reference to the buffer.
        /// </summary>
        /// <param name="reference">The reference to add.</param>
        /// <param name="diagnosticSource">The object that referenced the module.</param>
        /// <returns>The new metadata token assigned to the module reference.</returns>
        public MetadataToken GetModuleReferenceToken(ModuleReference? reference, object? diagnosticSource = null)
        {
            return AddModuleReference(reference, false, false, diagnosticSource);
        }

        /// <summary>
        /// Adds a module reference to the buffer.
        /// </summary>
        /// <param name="reference">The reference to add.</param>
        /// <param name="allowDuplicates">
        /// <c>true</c> if the row is always to be added to the end of the buffer, <c>false</c> if a duplicated row
        /// is supposed to be removed and the token of the original should be returned instead.
        /// </param>
        /// <param name="preserveRid">
        /// <c>true</c> if the metadata token of the module should be preserved, <c>false</c> otherwise.
        /// </param>
        /// <param name="diagnosticSource">The object that referenced the module.</param>
        /// <returns>The newly assigned metadata token.</returns>
        public MetadataToken AddModuleReference(ModuleReference? reference, bool allowDuplicates, bool preserveRid, object? diagnosticSource = null)
        {
            if (!AssertIsImported(reference, diagnosticSource))
                return MetadataToken.Zero;

            var table = Metadata.TablesStream.GetDistinctTable<ModuleReferenceRow>(TableIndex.ModuleRef);

            var row = new ModuleReferenceRow(Metadata.StringsStream.GetStringIndex(reference.Name));
            var token = preserveRid && reference.MetadataToken.Rid != 0
                ? table.Insert(reference.MetadataToken.Rid, row, allowDuplicates)
                : table.Add(row, allowDuplicates);

            AddCustomAttributes(token, reference);
            return token;
        }

        /// <inheritdoc />
        public MetadataToken GetTypeSpecificationToken(TypeSpecification? type, object? diagnosticSource = null)
        {
            return AddTypeSpecification(type, false, diagnosticSource);
        }

        /// <summary>
        /// Adds a type specification to the buffer.
        /// </summary>
        /// <param name="type">The specification to add.</param>
        /// <param name="allowDuplicates">
        /// <c>true</c> if the row is always to be added to the end of the buffer, <c>false</c> if a duplicated row
        /// is supposed to be removed and the token of the original should be returned instead.
        /// </param>
        /// <param name="diagnosticSource">The object that referenced the type specification.</param>
        /// <returns>The newly assigned metadata token.</returns>
        public MetadataToken AddTypeSpecification(TypeSpecification? type, bool allowDuplicates, object? diagnosticSource = null)
        {
            if (!AssertIsImported(type, diagnosticSource))
                return MetadataToken.Zero;

            var table = Metadata.TablesStream.GetDistinctTable<TypeSpecificationRow>(TableIndex.TypeSpec);
            var row = new TypeSpecificationRow(
                Metadata.BlobStream.GetBlobIndex(this, type.Signature, ErrorListener, diagnosticSource)
            );

            var token = table.Add(row, allowDuplicates);
            _tokenMapping.Register(type, token);
            AddCustomAttributes(token, type);
            return token;
        }

        /// <inheritdoc />
        public MetadataToken GetMethodSpecificationToken(MethodSpecification? method, object? diagnosticSource = null)
        {
            return AddMethodSpecification(method, false, diagnosticSource);
        }

        /// <summary>
        /// Adds a method specification to the buffer.
        /// </summary>
        /// <param name="method">The specification to add.</param>
        /// <param name="allowDuplicates">
        /// <c>true</c> if the row is always to be added to the end of the buffer, <c>false</c> if a duplicated row
        /// is supposed to be removed and the token of the original should be returned instead.
        /// </param>
        /// <param name="diagnosticSource">The object that referenced the method specification.</param>
        /// <returns>The newly assigned metadata token.</returns>
        public MetadataToken AddMethodSpecification(MethodSpecification? method, bool allowDuplicates, object? diagnosticSource = null)
        {
            if (!AssertIsImported(method, diagnosticSource))
                return MetadataToken.Zero;

            var table = Metadata.TablesStream.GetDistinctTable<MethodSpecificationRow>(TableIndex.MethodSpec);
            var row = new MethodSpecificationRow(
                AddMethodDefOrRef(method.Method, method),
                Metadata.BlobStream.GetBlobIndex(this, method.Signature, ErrorListener, diagnosticSource)
            );

            var token = table.Add(row, allowDuplicates);
            _tokenMapping.Register(method, token);
            AddCustomAttributes(token, method);
            return token;
        }
    }
}
