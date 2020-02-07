using System;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Blob;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Serialized
{
    /// <summary>
    /// Represents a lazily initialized implementation of <see cref="Constant"/>  that is read from a
    /// .NET metadata image. 
    /// </summary>
    public class SerializedConstant : Constant
    {
        private readonly SerializedModuleDefinition _parentModule;
        private readonly ConstantRow _row;

        /// <summary>
        /// Creates a constant from a constant metadata row.
        /// </summary>
        /// <param name="parentModule">The module that contains the constant.</param>
        /// <param name="token">The token to initialize the constant for.</param>
        /// <param name="row">The metadata table row to base the constant on.</param>
        public SerializedConstant(SerializedModuleDefinition parentModule, MetadataToken token,
            ConstantRow row)
            : base(token)
        {
            _parentModule = parentModule ?? throw new ArgumentNullException(nameof(parentModule));
            _row = row;

            Type = row.Type;
        }

        /// <inheritdoc />
        protected override IHasConstant GetParent()
        {
            var encoder = _parentModule.DotNetDirectory.Metadata
                .GetStream<TablesStream>()
                .GetIndexEncoder(CodedIndex.HasConstant);

            var token = encoder.DecodeIndex(_row.Parent);
            return _parentModule.TryLookupMember(token, out var member)
                ? member as IHasConstant
                : null;
        }

        /// <inheritdoc />
        protected override DataBlobSignature GetValue()
        {
            var reader = _parentModule.DotNetDirectory.Metadata
                .GetStream<BlobStream>()
                .GetBlobReaderByIndex(_row.Value);
            return DataBlobSignature.FromReader(reader);
        }
    }
}