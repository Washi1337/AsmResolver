using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Collections;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Blob;
using AsmResolver.PE.DotNet.Metadata.Strings;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Serialized
{
    /// <summary>
    /// Represents a lazily initialized implementation of <see cref="PropertyDefinition"/>  that is read from a
    /// .NET metadata image.
    /// </summary>
    public class SerializedPropertyDefinition : PropertyDefinition
    {
        private readonly ModuleReaderContext _context;
        private readonly PropertyDefinitionRow _row;

        /// <summary>
        /// Creates a property definition from a property metadata row.
        /// </summary>
        /// <param name="context">The reader context.</param>
        /// <param name="token">The token to initialize the property for.</param>
        /// <param name="row">The metadata table row to base the property definition on.</param>
        public SerializedPropertyDefinition(ModuleReaderContext context, MetadataToken token, in PropertyDefinitionRow row)
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
        protected override PropertySignature? GetSignature()
        {
            if (!_context.Metadata.TryGetStream<BlobStream>(out var blobStream)
                || !blobStream.TryGetBlobReaderByIndex(_row.Type, out var reader))
            {
                return _context.BadImageAndReturn<PropertySignature>(
                    $"Invalid signature blob index in property {MetadataToken.ToString()}.");
            }

            return PropertySignature.FromReader(new BlobReadContext(_context), ref reader);
        }

        /// <inheritdoc />
        protected override TypeDefinition? GetDeclaringType()
        {
            var module = _context.ParentModule;

            var declaringTypeToken = new MetadataToken(TableIndex.TypeDef, module.GetPropertyDeclaringType(MetadataToken.Rid));
            return module.TryLookupMember(declaringTypeToken, out var member)
                ? member as TypeDefinition
                : _context.BadImageAndReturn<TypeDefinition>(
                    $"Property {MetadataToken.ToString()} is not in the range of a property map of a declaring type.");
        }

        /// <inheritdoc />
        protected override IList<MethodSemantics> GetSemantics()
        {
            var module = _context.ParentModule;
            var rids = module.GetMethodSemantics(MetadataToken);

            var result = new MethodSemanticsCollection(this, rids.Count);
            result.ValidateMembership = false;

            foreach (uint rid in rids)
            {
                var semanticsToken = new MetadataToken(TableIndex.MethodSemantics, rid);
                result.Add((MethodSemantics) module.LookupMember(semanticsToken));
            }

            result.ValidateMembership = true;
            return result;
        }

        /// <inheritdoc />
        protected override IList<CustomAttribute> GetCustomAttributes() =>
            _context.ParentModule.GetCustomAttributeCollection(this);

        /// <inheritdoc />
        protected override Constant? GetConstant() =>
            _context.ParentModule.GetConstant(MetadataToken);
    }
}
