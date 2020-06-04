using System;
using System.Collections.Generic;
using AsmResolver.Collections;
using AsmResolver.DotNet.Collections;
using AsmResolver.PE.DotNet.Metadata.Strings;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Serialized
{
    /// <summary>
    /// Represents a lazily initialized implementation of <see cref="ManifestResource"/>  that is read from a
    /// .NET metadata image. 
    /// </summary>
    public class SerializedManifestResource : ManifestResource
    {
        private readonly SerializedModuleDefinition _parentModule;
        private readonly ManifestResourceRow _row;

        /// <summary>
        /// Creates a manifest resource from a manifest resource metadata row.
        /// </summary>
        /// <param name="parentModule">The module that contains the resource.</param>
        /// <param name="token">The token to initialize the resource for.</param>
        /// <param name="row">The metadata table row to base the resource \on.</param>
        public SerializedManifestResource(SerializedModuleDefinition parentModule, MetadataToken token, ManifestResourceRow row)
            : base(token)
        {
            _parentModule = parentModule ?? throw new ArgumentNullException(nameof(parentModule));
            _row = row;

            Attributes = row.Attributes;
            ((IOwnedCollectionElement<ModuleDefinition>) this).Owner = parentModule;
        }

        /// <inheritdoc />
        protected override string GetName()
        {
            return _parentModule.DotNetDirectory.Metadata
                .GetStream<StringsStream>()
                .GetStringByIndex(_row.Name);
        }

        /// <inheritdoc />
        protected override IImplementation GetImplementation()
        {
            if (_row.Implementation != 0)
                return null;

            var encoder = _parentModule.DotNetDirectory.Metadata
                .GetStream<TablesStream>()
                .GetIndexEncoder(CodedIndex.Implementation);

            var token = encoder.DecodeIndex(_row.Implementation);
            return _parentModule.TryLookupMember(token, out var member)
                ? member as IImplementation
                : null;
        }

        /// <inheritdoc />
        protected override ISegment GetEmbeddedDataSegment()
        {
            if (_row.Implementation != 0)
                return null;

            var reader = _parentModule.DotNetDirectory.DotNetResources.CreateManifestResourceReader(_row.Offset);
            return reader is null ? null : DataSegment.FromReader(reader);
        }

        /// <inheritdoc />
        protected override IList<CustomAttribute> GetCustomAttributes() => 
            _parentModule.GetCustomAttributeCollection(this);
    }
}