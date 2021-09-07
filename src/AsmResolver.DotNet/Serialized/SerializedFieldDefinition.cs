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
        private readonly ModuleReaderContext _context;
        private readonly FieldDefinitionRow _row;

        /// <summary>
        /// Creates a field definition from a field metadata row.
        /// </summary>
        /// <param name="context">The reader context.</param>
        /// <param name="token">The token to initialize the field for.</param>
        /// <param name="row">The metadata table row to base the field definition on.</param>
        public SerializedFieldDefinition(ModuleReaderContext context, MetadataToken token, in FieldDefinitionRow row)
            : base(token)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _row = row;

            Attributes = row.Attributes;
        }

        /// <inheritdoc />
        protected override Utf8String? GetName()
        {
            return _context.Metadata.TryGetStream<StringsStream>(out var stringsStream)
                ? stringsStream.GetStringByIndex(_row.Name)
                : null;
        }

        /// <inheritdoc />
        protected override FieldSignature? GetSignature()
        {
            if (!_context.Metadata.TryGetStream<BlobStream>(out var blobStream)
                || !blobStream.TryGetBlobReaderByIndex(_row.Signature, out var reader))
            {
                return _context.BadImageAndReturn<FieldSignature>(
                    $"Invalid signature blob index in field {MetadataToken.ToString()}.");
            }

            return FieldSignature.FromReader(new BlobReadContext(_context), ref reader);
        }

        /// <inheritdoc />
        protected override TypeDefinition? GetDeclaringType()
        {
            var module = _context.ParentModule;
            var declaringTypeToken = new MetadataToken(TableIndex.TypeDef, module.GetFieldDeclaringType(MetadataToken.Rid));
            return module.TryLookupMember(declaringTypeToken, out var member)
                ? member as TypeDefinition
                : _context.BadImageAndReturn<TypeDefinition>(
                    $"Field {MetadataToken.ToString()} is not in the range of a declaring type.");
        }

        /// <inheritdoc />
        protected override Constant? GetConstant() =>
            _context.ParentModule.GetConstant(MetadataToken);

        /// <inheritdoc />
        protected override MarshalDescriptor? GetMarshalDescriptor() =>
            _context.ParentModule.GetFieldMarshal(MetadataToken);

        /// <inheritdoc />
        protected override ImplementationMap? GetImplementationMap()
        {
            var module = _context.ParentModule;
            uint mapRid = module.GetImplementationMapRid(MetadataToken);
            return module.TryLookupMember(new MetadataToken(TableIndex.ImplMap, mapRid), out var member)
                ? member as ImplementationMap
                : null;
        }

        /// <inheritdoc />
        protected override ISegment? GetFieldRva()
        {
            var module = _context.ParentModule;

            uint rid = module.GetFieldRvaRid(MetadataToken);
            bool result = _context.Metadata
                .GetStream<TablesStream>()
                .GetTable<FieldRvaRow>()
                .TryGetByRid(rid, out var fieldRvaRow);

            return result
                ? _context.Parameters.FieldRvaDataReader.ResolveFieldData(_context, _context.Metadata, fieldRvaRow)
                : null;
        }

        /// <inheritdoc />
        protected override int? GetFieldOffset()
        {
            uint rid = _context.ParentModule.GetFieldLayoutRid(MetadataToken);
            bool result = _context.Metadata
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
