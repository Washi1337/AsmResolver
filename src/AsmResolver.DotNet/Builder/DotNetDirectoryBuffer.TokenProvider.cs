using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.Builder
{
    public partial class DotNetDirectoryBuffer : IMetadataTokenProvider
    {
        /// <inheritdoc />
        public uint GetUserStringIndex(string value) => Metadata.UserStringsStream.GetStringIndex(value);

        /// <inheritdoc />
        public MetadataToken GetTypeReferenceToken(TypeReference? type)
        {
            return AddTypeReference(type, false, false);
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
        /// <returns>The newly assigned metadata token.</returns>
        public MetadataToken AddTypeReference(TypeReference? type, bool allowDuplicates, bool preserveRid)
        {
            if (!AssertIsImported(type))
                return MetadataToken.Zero;

            var table = Metadata.TablesStream.GetDistinctTable<TypeReferenceRow>(TableIndex.TypeRef);
            var row = new TypeReferenceRow(
                AddResolutionScope(type.Scope, allowDuplicates, preserveRid),
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
        public MetadataToken GetTypeDefinitionToken(TypeDefinition? type)
        {
            return AssertIsImported(type)
                ? _tokenMapping[type]
                : MetadataToken.Zero;
        }

        /// <inheritdoc />
        public MetadataToken GetFieldDefinitionToken(FieldDefinition? field)
        {
            return AssertIsImported(field)
                ? _tokenMapping[field]
                : MetadataToken.Zero;
        }

        /// <inheritdoc />
        public MetadataToken GetMethodDefinitionToken(MethodDefinition? method)
        {
            return AssertIsImported(method)
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
            return AssertIsImported(parameter)
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
            return AssertIsImported(property)
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
            return AssertIsImported(@event)
                ? _tokenMapping[@event]
                : MetadataToken.Zero;
        }

        /// <inheritdoc />
        public MetadataToken GetMemberReferenceToken(MemberReference? member)
        {
            return AddMemberReference(member, false);
        }

        /// <summary>
        /// Adds a member reference to the buffer.
        /// </summary>
        /// <param name="member">The reference to add.</param>
        /// <param name="allowDuplicates">
        /// <c>true</c> if the row is always to be added to the end of the buffer, <c>false</c> if a duplicated row
        /// is supposed to be removed and the token of the original should be returned instead.
        /// </param>
        /// <returns>The newly assigned metadata token.</returns>
        public MetadataToken AddMemberReference(MemberReference? member, bool allowDuplicates)
        {
            if (!AssertIsImported(member))
                return MetadataToken.Zero;

            var table = Metadata.TablesStream.GetDistinctTable<MemberReferenceRow>(TableIndex.MemberRef);
            var row = new MemberReferenceRow(
                AddMemberRefParent(member.Parent),
                Metadata.StringsStream.GetStringIndex(member.Name),
                Metadata.BlobStream.GetBlobIndex(this, member.Signature, ErrorListener));

            var token = table.Add(row, allowDuplicates);
            _tokenMapping.Register(member, token);
            AddCustomAttributes(token, member);
            return token;
        }

        /// <inheritdoc />
        public MetadataToken GetStandAloneSignatureToken(StandAloneSignature? signature)
        {
            return AddStandAloneSignature(signature, false);
        }

        /// <summary>
        /// Adds a stand-alone signature to the buffer.
        /// </summary>
        /// <param name="signature">The signature to add.</param>
        /// <param name="allowDuplicates">
        /// <c>true</c> if the row is always to be added to the end of the buffer, <c>false</c> if a duplicated row
        /// is supposed to be removed and the token of the original should be returned instead.
        /// </param>
        /// <returns>The newly assigned metadata token.</returns>
        public MetadataToken AddStandAloneSignature(StandAloneSignature? signature, bool allowDuplicates)
        {
            if (signature is null)
                return MetadataToken.Zero;

            var table = Metadata.TablesStream.GetDistinctTable<StandAloneSignatureRow>(TableIndex.StandAloneSig);
            var row = new StandAloneSignatureRow(
                Metadata.BlobStream.GetBlobIndex(this, signature.Signature, ErrorListener));

            var token = table.Add(row, allowDuplicates);
            _tokenMapping.Register(signature, token);
            AddCustomAttributes(token, signature);
            return token;
        }

        /// <inheritdoc />
        public MetadataToken GetAssemblyReferenceToken(AssemblyReference? assembly)
        {
            return AddAssemblyReference(assembly, false, false);
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
        /// <returns>The newly assigned metadata token.</returns>
        public MetadataToken AddAssemblyReference(AssemblyReference? assembly, bool allowDuplicates, bool preserveRid)
        {
            if (assembly is null || !AssertIsImported(assembly))
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
        /// <returns>The new metadata token assigned to the module reference.</returns>
        public MetadataToken GetModuleReferenceToken(ModuleReference? reference)
        {
            return AddModuleReference(reference, false, false);
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
        /// <returns>The newly assigned metadata token.</returns>
        public MetadataToken AddModuleReference(ModuleReference? reference, bool allowDuplicates, bool preserveRid)
        {
            if (!AssertIsImported(reference))
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
        public MetadataToken GetTypeSpecificationToken(TypeSpecification? type)
        {
            return AddTypeSpecification(type, false);
        }

        /// <summary>
        /// Adds a type specification to the buffer.
        /// </summary>
        /// <param name="type">The specification to add.</param>
        /// <param name="allowDuplicates">
        /// <c>true</c> if the row is always to be added to the end of the buffer, <c>false</c> if a duplicated row
        /// is supposed to be removed and the token of the original should be returned instead.
        /// </param>
        /// <returns>The newly assigned metadata token.</returns>
        public MetadataToken AddTypeSpecification(TypeSpecification? type, bool allowDuplicates)
        {
            if (!AssertIsImported(type))
                return MetadataToken.Zero;

            var table = Metadata.TablesStream.GetDistinctTable<TypeSpecificationRow>(TableIndex.TypeSpec);
            var row = new TypeSpecificationRow(Metadata.BlobStream.GetBlobIndex(this, type.Signature, ErrorListener));

            var token = table.Add(row, allowDuplicates);
            _tokenMapping.Register(type, token);
            AddCustomAttributes(token, type);
            return token;
        }

        /// <inheritdoc />
        public MetadataToken GetMethodSpecificationToken(MethodSpecification? method)
        {
            return AddMethodSpecification(method, false);
        }

        /// <summary>
        /// Adds a method specification to the buffer.
        /// </summary>
        /// <param name="method">The specification to add.</param>
        /// <param name="allowDuplicates">
        /// <c>true</c> if the row is always to be added to the end of the buffer, <c>false</c> if a duplicated row
        /// is supposed to be removed and the token of the original should be returned instead.
        /// </param>
        /// <returns>The newly assigned metadata token.</returns>
        public MetadataToken AddMethodSpecification(MethodSpecification? method, bool allowDuplicates)
        {
            if (!AssertIsImported(method))
                return MetadataToken.Zero;

            var table = Metadata.TablesStream.GetDistinctTable<MethodSpecificationRow>(TableIndex.MethodSpec);
            var row = new MethodSpecificationRow(
                AddMethodDefOrRef(method.Method),
                Metadata.BlobStream.GetBlobIndex(this, method.Signature, ErrorListener));

            var token = table.Add(row, allowDuplicates);
            _tokenMapping.Register(method, token);
            AddCustomAttributes(token, method);
            return token;
        }
    }
}
