using System;
using System.Collections.Generic;
using AsmResolver.DotNet.Collections;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Serialized
{
    /// <summary>
    /// Represents a lazily initialized implementation of <see cref="InterfaceImplementation"/>  that is read from a
    /// .NET metadata image. 
    /// </summary>
    public class SerializedInterfaceImplementation : InterfaceImplementation
    {
        private readonly SerializedModuleDefinition _parentModule;
        private readonly InterfaceImplementationRow _row;

        /// <summary>
        /// Creates a interface implementation from an interface implementation metadata row.
        /// </summary>
        /// <param name="parentModule">The module that contains the interface implementation.</param>
        /// <param name="token">The token to initialize the interface implementation for.</param>
        /// <param name="row">The metadata table row to base the interface implementation on.</param>
        public SerializedInterfaceImplementation(SerializedModuleDefinition parentModule,
            MetadataToken token, InterfaceImplementationRow row)
            : base(token)
        {
            _parentModule = parentModule ?? throw new ArgumentNullException(nameof(parentModule));
            _row = row;
        }

        /// <inheritdoc />
        protected override TypeDefinition GetClass()
        {
            var token = _parentModule.GetInterfaceImplementationOwner(MetadataToken.Rid);
            return _parentModule.TryLookupMember(token, out var member)
                ? member as TypeDefinition
                : null;
        }

        /// <inheritdoc />
        protected override ITypeDefOrRef GetInterface()
        {
            var encoder = _parentModule.DotNetDirectory.Metadata
                .GetStream<TablesStream>()
                .GetIndexEncoder(CodedIndex.TypeDefOrRef);
            var token = encoder.DecodeIndex(_row.Interface);
            
            return _parentModule.TryLookupMember(token, out var member)
                ? member as ITypeDefOrRef
                : null;
        }

        /// <inheritdoc />
        protected override IList<CustomAttribute> GetCustomAttributes() => 
            _parentModule.GetCustomAttributeCollection(this);
    }
}