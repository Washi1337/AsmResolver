using System;
using System.Collections.Generic;
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
        protected override string GetName() => _context.Image.DotNetDirectory.Metadata
            .GetStream<StringsStream>()
            .GetStringByIndex(_row.Name);

        /// <inheritdoc />
        protected override PropertySignature GetSignature()
        {
            var reader = _context.Image.DotNetDirectory.Metadata
                .GetStream<BlobStream>()
                .GetBlobReaderByIndex(_row.Type);
            
            return PropertySignature.FromReader(new BlobReadContext(_context), reader);
        }

        /// <inheritdoc />
        protected override TypeDefinition GetDeclaringType()
        {
            var module = _context.ParentModule;
            
            var declaringTypeToken = new MetadataToken(TableIndex.TypeDef, module.GetPropertyDeclaringType(MetadataToken.Rid));
            return module.TryLookupMember(declaringTypeToken, out var member)
                ? member as TypeDefinition
                : null;
        }

        /// <inheritdoc />
        protected override IList<MethodSemantics> GetSemantics()
        {
            var result = new MethodSemanticsCollection(this);
            result.ValidateMembership = false;

            var module = _context.ParentModule;
            foreach (uint rid in module.GetMethodSemantics(MetadataToken))
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
        protected override Constant GetConstant() => 
            _context.ParentModule.GetConstant(MetadataToken);
    }
}