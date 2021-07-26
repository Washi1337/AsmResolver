using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace AsmResolver.DotNet.Memory
{
    /// <summary>
    /// Provides information about the memory layout of a type.
    /// </summary>
    public class TypeMemoryLayout
    {
        private readonly Dictionary<FieldDefinition, FieldMemoryLayout> _fields = new();

        internal TypeMemoryLayout(ITypeDescriptor type)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }

        /// <summary>
        /// Creates a new instance of the <see cref="TypeMemoryLayout"/> class.
        /// </summary>
        /// <param name="type">The type for which the memory layout is determined.</param>
        /// <param name="size">The size of the type.</param>
        /// <param name="attributes">The attributes</param>
        public TypeMemoryLayout(ITypeDescriptor type, uint size, MemoryLayoutAttributes attributes)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Size = size;
            Attributes = attributes;
        }

        /// <summary>
        /// Gets the type for which the memory layout was determined.
        /// </summary>
        public ITypeDescriptor Type
        {
            get;
        }

        /// <summary>
        /// Gets the implied memory layout for the provided field.
        /// </summary>
        /// <param name="field">The field.</param>
        public FieldMemoryLayout this[FieldDefinition field]
        {
            get => _fields[field];
            internal set => _fields[field] = value;
        }

        /// <summary>
        /// Gets the total number of bytes this structure requires.
        /// </summary>
        public uint Size
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets additional attributes associated to this memory layout.
        /// </summary>
        public MemoryLayoutAttributes Attributes
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets a value indicating whether the type layout was determined assuming a 32-bit environment.
        /// </summary>
        public bool Is32Bit => (Attributes & MemoryLayoutAttributes.BitnessMask) == MemoryLayoutAttributes.Is32Bit;

        /// <summary>
        /// Gets a value indicating whether the type layout was determined assuming a 64-bit environment.
        /// </summary>
        public bool Is64Bit => (Attributes & MemoryLayoutAttributes.BitnessMask) == MemoryLayoutAttributes.Is64Bit;

        /// <summary>
        /// Gets a value indicating whether the type layout is dependent on the bitness of the environment.
        /// </summary>
        public bool IsPlatformDependent => (Attributes & MemoryLayoutAttributes.IsPlatformDependent) != 0;

        private IEnumerable<FieldMemoryLayout> GetOrderedFields()
        {
            return _fields.Values
                .OrderBy(f => f.Offset)
                .ThenBy(f => f.ContentsLayout.Size);
        }

        /// <summary>
        /// Finds a field within this type memory layout by its offset.
        /// </summary>
        /// <param name="offset">The offset of the field to find.</param>
        /// <param name="field">When the method returns <c>true</c>, contains the field with the provided offset..</param>
        /// <returns><c>true</c> if the field with the provided offset existed, <c>false,</c> otherwise.</returns>
        public bool TryGetFieldAtOffset(uint offset, [NotNullWhen(true)] out FieldMemoryLayout? field)
        {
            foreach (var entry in GetOrderedFields())
            {
                if (entry.Offset != offset)
                    continue;
                field = entry;
                return true;
            }

            field = null;
            return false;
        }

        /// <summary>
        /// Traverses the type memory layout tree and finds a field within this type memory layout by its offset.
        /// </summary>
        /// <param name="offset">The offset of the field to find.</param>
        /// <param name="path">The (incomplete) path of fields to traverse to reach the provided field offset.</param>
        /// <returns><c>true</c> if the offset points to the start of a field, <c>false</c> otherwise.</returns>
        public bool TryGetFieldPath(uint offset, out IList<FieldMemoryLayout> path)
        {
            path = new List<FieldMemoryLayout>();

            var lastExactOffsetMatch = default(FieldMemoryLayout);
            var currentLayout = this;

            bool stop = false;
            while (!stop)
            {
                stop = true;

                foreach (var entry in currentLayout.GetOrderedFields())
                {
                    if (offset < entry.Offset)
                        continue;

                    if (offset < entry.Offset + entry.ContentsLayout.Size)
                    {
                        // We found the field that contains the offset. Dive into this field.

                        if (entry.Offset == offset)
                            lastExactOffsetMatch = entry;

                        path.Add(entry);
                        currentLayout = entry.ContentsLayout;
                        offset -= entry.Offset;

                        stop = false;
                        break;
                    }
                }
            }

            return path.Count > 0 && path[path.Count - 1] == lastExactOffsetMatch;
        }
    }
}
