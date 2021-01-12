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
        private readonly ModuleReadContext _context;
        private readonly InterfaceImplementationRow _row;

        /// <summary>
        /// Creates a interface implementation from an interface implementation metadata row.
        /// </summary>
        /// <param name="context">The reader context.</param>
        /// <param name="token">The token to initialize the interface implementation for.</param>
        /// <param name="row">The metadata table row to base the interface implementation on.</param>
        public SerializedInterfaceImplementation(
            ModuleReadContext context,
            MetadataToken token,
            in InterfaceImplementationRow row)
            : base(token)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _row = row;
        }

        /// <inheritdoc />
        protected override TypeDefinition GetClass()
        {
            var module = _context.ParentModule;
            var token = module.GetInterfaceImplementationOwner(MetadataToken.Rid);
            return module.TryLookupMember(token, out var member)
                ? member as TypeDefinition
                : null;
        }

        /// <inheritdoc />
        protected override ITypeDefOrRef GetInterface()
        {
            var encoder = _context.Image.DotNetDirectory.Metadata
                .GetStream<TablesStream>()
                .GetIndexEncoder(CodedIndex.TypeDefOrRef);
            var token = encoder.DecodeIndex(_row.Interface);
            
            return _context.ParentModule.TryLookupMember(token, out var member)
                ? member as ITypeDefOrRef
                : null;
        }

        /// <inheritdoc />
        protected override IList<CustomAttribute> GetCustomAttributes() => 
            _context.ParentModule.GetCustomAttributeCollection(this);
    }
}