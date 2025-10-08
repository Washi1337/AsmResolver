using System;
using System.Collections.Generic;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.Serialized
{
    /// <summary>
    /// Represents a lazily initialized implementation of <see cref="InterfaceImplementation"/>  that is read from a
    /// .NET metadata image.
    /// </summary>
    public class SerializedInterfaceImplementation : InterfaceImplementation
    {
        private readonly ModuleReaderContext _context;
        private readonly InterfaceImplementationRow _row;

        /// <summary>
        /// Creates a interface implementation from an interface implementation metadata row.
        /// </summary>
        /// <param name="context">The reader context.</param>
        /// <param name="token">The token to initialize the interface implementation for.</param>
        /// <param name="row">The metadata table row to base the interface implementation on.</param>
        public SerializedInterfaceImplementation(
            ModuleReaderContext context,
            MetadataToken token,
            in InterfaceImplementationRow row)
            : base(token)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _row = row;
        }

        /// <inheritdoc />
        public override bool HasCustomAttributes => CustomAttributesInternal is null
            ? _context.ParentModule.HasNonEmptyCustomAttributes(this)
            : CustomAttributes.Count > 0;

        /// <inheritdoc />
        protected override TypeDefinition? GetClass()
        {
            var module = _context.ParentModule;
            var token = module.GetInterfaceImplementationOwner(MetadataToken.Rid);
            return module.TryLookupMember(token, out var member)
                ? member as TypeDefinition
                : _context.BadImageAndReturn<TypeDefinition>(
                    $"Invalid parent class in interface implementation {MetadataToken.ToString()}.");
        }

        /// <inheritdoc />
        protected override ITypeDefOrRef? GetInterface()
        {
            var token = _context.TablesStream
                .GetIndexEncoder(CodedIndex.TypeDefOrRef)
                .DecodeIndex(_row.Interface);

            return _context.ParentModule.TryLookupMember(token, out var member)
                ? member as ITypeDefOrRef
                : _context.BadImageAndReturn<TypeDefinition>(
                    $"Invalid interface in interface implementation {MetadataToken.ToString()}.");
        }

        /// <inheritdoc />
        protected override IList<CustomAttribute> GetCustomAttributes() =>
            _context.ParentModule.GetCustomAttributeCollection(this);
    }
}
