using System;
using System.Collections.Generic;
using AsmResolver.DotNet.Collections;
using AsmResolver.PE.DotNet.Metadata.Strings;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Serialized
{
    /// <summary>
    /// Represents a lazily initialized implementation of <see cref="ModuleReference"/>  that is read from a
    /// .NET metadata image. 
    /// </summary>
    public class SerializedModuleReference : ModuleReference
    {
        private readonly SerializedModuleDefinition _parentModule;
        private readonly ModuleReferenceRow _row;

        /// <summary>
        /// Creates a module reference from a module reference metadata row.
        /// </summary>
        /// <param name="parentModule">The module that contains the module reference.</param>
        /// <param name="token">The token to initialize the module reference for.</param>
        /// <param name="row">The metadata table row to base the module reference. on.</param>
        public SerializedModuleReference(SerializedModuleDefinition parentModule, MetadataToken token, ModuleReferenceRow row)
            : base(token)
        {
            _parentModule = parentModule ?? throw new ArgumentNullException(nameof(parentModule));
            _row = row;

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
        protected override IList<CustomAttribute> GetCustomAttributes() => 
            _parentModule.GetCustomAttributeCollection(this);
    }
}