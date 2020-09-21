using System;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Serialized
{
    /// <summary>
    /// Represents a lazily initialized implementation of <see cref="MethodSemantics"/>  that is read from a
    /// .NET metadata image. 
    /// </summary>
    public class SerializedMethodSemantics : MethodSemantics
    {
        private readonly SerializedModuleDefinition _parentModule;
        private readonly MethodSemanticsRow _row;

        /// <summary>
        /// Creates a method semantics object from a method semantics row.
        /// </summary>
        /// <param name="parentModule"></param>
        /// <param name="token">The token to initialize the semantics for.</param>
        /// <param name="row">The metadata table row to base the semantics on.</param>
        public SerializedMethodSemantics(SerializedModuleDefinition parentModule, MetadataToken token, MethodSemanticsRow row)
            : base(token)
        {
            _parentModule = parentModule ?? throw new ArgumentNullException(nameof(parentModule));
            _row = row;
            
            Attributes = row.Attributes;
        }

        /// <inheritdoc />
        protected override MethodDefinition GetMethod()
        {
            var token = new MetadataToken(TableIndex.Method, _row.Method);
            return _parentModule.TryLookupMember(token, out var member)
                ? member as MethodDefinition
                : null;
        }

        /// <inheritdoc />
        protected override IHasSemantics GetAssociation()
        {
            var encoder = _parentModule.DotNetDirectory.Metadata
                .GetStream<TablesStream>()
                .GetIndexEncoder(CodedIndex.HasSemantics);
            
            var token = encoder.DecodeIndex(_row.Association);
            return _parentModule.TryLookupMember(token, out var member)
                ? member as IHasSemantics
                : null;
        }
    }
}