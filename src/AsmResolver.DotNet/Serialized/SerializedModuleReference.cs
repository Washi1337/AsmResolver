using System;
using System.Collections.Generic;
using AsmResolver.Collections;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.Serialized
{
    /// <summary>
    /// Represents a lazily initialized implementation of <see cref="ModuleReference"/>  that is read from a
    /// .NET metadata image.
    /// </summary>
    public class SerializedModuleReference : ModuleReference
    {
        private readonly ModuleReaderContext _context;
        private readonly ModuleReferenceRow _row;

        /// <summary>
        /// Creates a module reference from a module reference metadata row.
        /// </summary>
        /// <param name="context">The reader context.</param>
        /// <param name="token">The token to initialize the module reference for.</param>
        /// <param name="row">The metadata table row to base the module reference. on.</param>
        public SerializedModuleReference(ModuleReaderContext context, MetadataToken token, in ModuleReferenceRow row)
            : base(token)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _row = row;

            ((IOwnedCollectionElement<ModuleDefinition>) this).Owner = context.ParentModule;
        }

        /// <inheritdoc />
        public override bool HasCustomAttributes => CustomAttributesInternal is null
            ? _context.ParentModule.HasNonEmptyCustomAttributes(this)
            : CustomAttributes.Count > 0;

        /// <inheritdoc />
        protected override Utf8String? GetName() => _context.StringsStream?.GetStringByIndex(_row.Name);

        /// <inheritdoc />
        protected override IList<CustomAttribute> GetCustomAttributes() =>
            _context.ParentModule.GetCustomAttributeCollection(this);
    }
}
