using System;
using System.Collections.Generic;
using AsmResolver.Collections;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Code;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Collections;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Blob;
using AsmResolver.PE.DotNet.Metadata.Strings;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Serialized
{
    /// <summary>
    /// Represents a lazily initialized implementation of <see cref="MethodDefinition"/>  that is read from a
    /// .NET metadata image.
    /// </summary>
    public class SerializedMethodDefinition : MethodDefinition
    {
        private readonly ModuleReaderContext _context;
        private readonly MethodDefinitionRow _row;

        /// <summary>
        /// Creates a method definition from a method metadata row.
        /// </summary>
        /// <param name="context">The reader context.</param>
        /// <param name="token">The token to initialize the method for.</param>
        /// <param name="row">The metadata table row to base the method definition on.</param>
        public SerializedMethodDefinition(ModuleReaderContext context, MetadataToken token, in MethodDefinitionRow row)
            : base(token)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _row = row;

            Attributes = row.Attributes;
            ImplAttributes = row.ImplAttributes;
        }

        /// <inheritdoc />
        protected override Utf8String? GetName()
        {
            return _context.Metadata.TryGetStream<StringsStream>(out var stringsStream)
                ? stringsStream.GetStringByIndex(_row.Name)
                : null;
        }

        /// <inheritdoc />
        protected override MethodSignature? GetSignature()
        {
            if (!_context.Metadata.TryGetStream<BlobStream>(out var blobStream)
                || !blobStream.TryGetBlobReaderByIndex(_row.Signature, out var reader))
            {
                return _context.BadImageAndReturn<MethodSignature>(
                    $"Invalid signature blob index in method {MetadataToken.ToString()}.");
            }

            return MethodSignature.FromReader(new BlobReadContext(_context), ref reader);
        }

        /// <inheritdoc />
        protected override IList<CustomAttribute> GetCustomAttributes() =>
            _context.ParentModule.GetCustomAttributeCollection(this);

        /// <inheritdoc />
        protected override IList<SecurityDeclaration> GetSecurityDeclarations() =>
            _context.ParentModule.GetSecurityDeclarationCollection(this);

        /// <inheritdoc />
        protected override TypeDefinition? GetDeclaringType()
        {
            var declaringTypeToken = new MetadataToken(TableIndex.TypeDef, _context.ParentModule.GetMethodDeclaringType(MetadataToken.Rid));
            return _context.ParentModule.TryLookupMember(declaringTypeToken, out var member)
                ? member as TypeDefinition
                : _context.BadImageAndReturn<TypeDefinition>(
                    $"Method {MetadataToken.ToString()} is not in the range of a declaring type.");
        }

        /// <inheritdoc />
        protected override IList<ParameterDefinition> GetParameterDefinitions()
        {
            var result = new OwnedCollection<MethodDefinition, ParameterDefinition>(this);

            foreach (var token in _context.ParentModule.GetParameterRange(MetadataToken.Rid))
            {
                if (_context.ParentModule.TryLookupMember(token, out var member) && member is ParameterDefinition parameter)
                    result.Add(parameter);
            }

            return result;
        }

        /// <inheritdoc />
        protected override MethodBody? GetBody() =>
            _context.Parameters.MethodBodyReader.ReadMethodBody(_context, this, _row);

        /// <inheritdoc />
        protected override ImplementationMap? GetImplementationMap()
        {
            uint mapRid = _context.ParentModule.GetImplementationMapRid(MetadataToken);
            return _context.ParentModule.TryLookupMember(new MetadataToken(TableIndex.ImplMap, mapRid), out var member)
                ? member as ImplementationMap
                : null;
        }

        /// <inheritdoc />
        protected override IList<GenericParameter> GetGenericParameters()
        {
            var result = new OwnedCollection<IHasGenericParameters, GenericParameter>(this);

            foreach (uint rid in _context.ParentModule.GetGenericParameters(MetadataToken))
            {
                if (_context.ParentModule.TryLookupMember(new MetadataToken(TableIndex.GenericParam, rid), out var member)
                    && member is GenericParameter genericParameter)
                {
                    result.Add(genericParameter);
                }
            }

            return result;
        }

        /// <inheritdoc />
        protected override MethodSemantics? GetSemantics()
        {
            var ownerToken = _context.ParentModule.GetMethodParentSemantics(MetadataToken.Rid);
            return _context.ParentModule.TryLookupMember(ownerToken, out var member)
                ? member as MethodSemantics
                : null;
        }

        /// <inheritdoc />
        protected override UnmanagedExportInfo? GetExportInfo() => _context.ParentModule.GetExportInfo(MetadataToken);
    }
}
