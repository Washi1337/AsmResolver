using System;
using System.Collections.Generic;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Marshal;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Blob;
using AsmResolver.PE.DotNet.Metadata.Strings;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Serialized
{
    /// <summary>
    /// Represents a lazily initialized implementation of <see cref="FieldDefinition"/>  that is read from a
    /// .NET metadata image. 
    /// </summary>
    public class SerializedFieldDefinition : FieldDefinition
    {
        private readonly ModuleReadContext _context;
        private readonly FieldDefinitionRow _row;

        /// <summary>
        /// Creates a field definition from a field metadata row.
        /// </summary>
        /// <param name="context">The reader context.</param>
        /// <param name="token">The token to initialize the field for.</param>
        /// <param name="row">The metadata table row to base the field definition on.</param>
        public SerializedFieldDefinition(ModuleReadContext context, MetadataToken token, in FieldDefinitionRow row)
            : base(token)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _row = row;

            Attributes = row.Attributes;
        }

        /// <inheritdoc />
        protected override string GetName() => _context.Image.DotNetDirectory.Metadata
            .GetStream<StringsStream>()
            .GetStringByIndex(_row.Name);

        /// <inheritdoc />
        protected override FieldSignature GetSignature() => FieldSignature.FromReader(
            new BlobReadContext(_context),
            _context.Image.DotNetDirectory.Metadata
                .GetStream<BlobStream>()
                .GetBlobReaderByIndex(_row.Signature));

        /// <inheritdoc />
        protected override TypeDefinition GetDeclaringType()
        {
            var module = _context.ParentModule;
            var declaringTypeToken = new MetadataToken(TableIndex.TypeDef, module.GetFieldDeclaringType(MetadataToken.Rid));
            return module.TryLookupMember(declaringTypeToken, out var member)
                ? member as TypeDefinition
                : null;
        }

        /// <inheritdoc />
        protected override Constant GetConstant() =>
            _context.ParentModule.GetConstant(MetadataToken);

        /// <inheritdoc />
        protected override MarshalDescriptor GetMarshalDescriptor() =>
            _context.ParentModule.GetFieldMarshal(MetadataToken);
        
        /// <inheritdoc />
        protected override ImplementationMap GetImplementationMap()
        {
            var module = _context.ParentModule;
            uint mapRid = module.GetImplementationMapRid(MetadataToken);
            return module.TryLookupMember(new MetadataToken(TableIndex.ImplMap, mapRid), out var member)
                ? member as ImplementationMap
                : null;
        }

        /// <inheritdoc />
        protected override ISegment GetFieldRva()
        {
            var module = _context.ParentModule;
            
            uint rid = module.GetFieldRvaRid(MetadataToken);
            bool result = module.DotNetDirectory.Metadata
                .GetStream<TablesStream>()
                .GetTable<FieldRvaRow>()
                .TryGetByRid(rid, out var fieldRvaRow);
            
            if (!result)
                return null;

            return _context.Parameters.FieldRvaDataReader
                .ResolveFieldData(ThrowErrorListener.Instance, _context.Image.DotNetDirectory.Metadata, fieldRvaRow);
        }

        /// <inheritdoc />
        protected override int? GetFieldOffset()
        {
            uint rid = _context.ParentModule.GetFieldLayoutRid(MetadataToken);
            bool result = _context.Image.DotNetDirectory.Metadata
                .GetStream<TablesStream>()
                .GetTable<FieldLayoutRow>()
                .TryGetByRid(rid, out var fieldLayoutRow);
            
            if (!result)
                return null;

            return (int?) fieldLayoutRow.Offset;
        }

        /// <inheritdoc />
        protected override IList<CustomAttribute> GetCustomAttributes() => 
            _context.ParentModule.GetCustomAttributeCollection(this);
    }
}