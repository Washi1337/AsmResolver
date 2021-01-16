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
        private readonly ModuleReaderContext _context;
        private readonly MethodSemanticsRow _row;

        /// <summary>
        /// Creates a method semantics object from a method semantics row.
        /// </summary>
        /// <param name="context">The reader context.</param>
        /// <param name="token">The token to initialize the semantics for.</param>
        /// <param name="row">The metadata table row to base the semantics on.</param>
        public SerializedMethodSemantics(ModuleReaderContext context, MetadataToken token, in MethodSemanticsRow row)
            : base(token)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _row = row;
            
            Attributes = row.Attributes;
        }

        /// <inheritdoc />
        protected override MethodDefinition GetMethod()
        {
            var token = new MetadataToken(TableIndex.Method, _row.Method);
            return _context.ParentModule.TryLookupMember(token, out var member)
                ? member as MethodDefinition
                : null;
        }

        /// <inheritdoc />
        protected override IHasSemantics GetAssociation()
        {
            var encoder = _context.Image.DotNetDirectory.Metadata
                .GetStream<TablesStream>()
                .GetIndexEncoder(CodedIndex.HasSemantics);
            
            var token = encoder.DecodeIndex(_row.Association);
            return _context.ParentModule.TryLookupMember(token, out var member)
                ? member as IHasSemantics
                : null;
        }
    }
}