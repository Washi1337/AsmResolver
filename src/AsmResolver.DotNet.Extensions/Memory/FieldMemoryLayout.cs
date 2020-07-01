using System;

namespace AsmResolver.DotNet.Memory
{
    /// <summary>
    /// Provides information about the layout of a single field in a type.
    /// </summary>
    public class FieldMemoryLayout
    {
        /// <summary>
        /// Creates a new instance of the <see cref="FieldMemoryLayout"/> class.
        /// </summary>
        /// <param name="field">The field that was laid out.</param>
        /// <param name="offset">The offset of the field.</param>
        /// <param name="contentsLayout">The layout of the contents of the field.</param>
        public FieldMemoryLayout(FieldDefinition field, uint offset, TypeMemoryLayout contentsLayout)
        {
            Field = field ?? throw new ArgumentNullException(nameof(field));
            Offset = offset;
            ContentsLayout = contentsLayout;
        }

        /// <summary>
        /// Gets the field that was aligned.
        /// </summary>
        public FieldDefinition Field
        {
            get;
        }

        /// <summary>
        /// Gets the implied offset of the field, relative to the start of the enclosing structure.
        /// </summary>
        public uint Offset
        {
            get;
        }

        /// <summary>
        /// Gets the layout of the contents of the field.
        /// </summary>
        public TypeMemoryLayout ContentsLayout
        {
            get;
        }
    }
}