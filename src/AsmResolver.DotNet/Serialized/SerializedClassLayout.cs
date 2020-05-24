using System;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Serialized
{
    /// <summary>
    /// Represents a lazily initialized implementation of <see cref="ClassLayout"/>  that is read from a
    /// .NET metadata image. 
    /// </summary>
    public class SerializedClassLayout : ClassLayout
    {
        private readonly SerializedModuleDefinition _parentModule;
        private readonly ClassLayoutRow _row;

        /// <summary>
        /// Creates a class layout from a class layout metadata row.
        /// </summary>
        /// <param name="parentModule">The module that contains the class layout.</param>
        /// <param name="token">The token to initialize the class layout for.</param>
        /// <param name="row">The metadata table row to base the class layout on.</param>
        public SerializedClassLayout(SerializedModuleDefinition parentModule, MetadataToken token,
            ClassLayoutRow row)
            : base(token)
        {
            _parentModule = parentModule ?? throw new ArgumentNullException(nameof(parentModule));
            _row = row;

            PackingSize = row.PackingSize;
            ClassSize = row.ClassSize;
        }

        /// <inheritdoc />
        protected override TypeDefinition GetParent()
        {
            return _parentModule.TryLookupMember(new MetadataToken(TableIndex.TypeDef, _row.Parent), out var member)
                ? member as TypeDefinition
                : null;
        }
    }
}