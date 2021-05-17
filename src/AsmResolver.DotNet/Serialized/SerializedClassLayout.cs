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
        private readonly ModuleReaderContext _context;
        private readonly ClassLayoutRow _row;

        /// <summary>
        /// Creates a class layout from a class layout metadata row.
        /// </summary>
        /// <param name="context">The reader context.</param>
        /// <param name="token">The token to initialize the class layout for.</param>
        /// <param name="row">The metadata table row to base the class layout on.</param>
        public SerializedClassLayout(
            ModuleReaderContext context,
            MetadataToken token,
            in ClassLayoutRow row)
            : base(token)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _row = row;

            PackingSize = row.PackingSize;
            ClassSize = row.ClassSize;
        }

        /// <inheritdoc />
        protected override TypeDefinition GetParent()
        {
            return _context.ParentModule.TryLookupMember(new MetadataToken(TableIndex.TypeDef, _row.Parent), out var member)
                ? member as TypeDefinition
                : _context.BadImageAndReturn<TypeDefinition>($"Invalid parent type in class layout {MetadataToken.ToString()}.");
        }
    }
}
