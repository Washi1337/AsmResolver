using System;
using System.Collections.Generic;
using System.Linq;

namespace AsmResolver.DotNet.Memory
{
    /// <summary>
    /// Provides information about the memory layout of a type.
    /// </summary>
    public class TypeMemoryLayout
    {
        private readonly Dictionary<FieldDefinition, FieldMemoryLayout> _fields =
            new Dictionary<FieldDefinition, FieldMemoryLayout>();

        internal TypeMemoryLayout(ITypeDescriptor type)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }

        /// <summary>
        /// Creates a new instance of the <see cref="TypeMemoryLayout"/> class.
        /// </summary>
        /// <param name="type">The type for which the memory layout is determined.</param>
        /// <param name="size">The size of the type.</param>
        public TypeMemoryLayout(ITypeDescriptor type, uint size)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Size = size;
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
        public bool TryGetFieldAtOffset(uint offset, out FieldMemoryLayout field)
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

            var currentLayout = this;

            bool stop = false;
            while (!stop)
            {
                stop = true;
                
                foreach (var entry in currentLayout.GetOrderedFields())
                {
                    if (offset >= entry.Offset)
                    {
                        // We're past any potential candidate.
                        if (offset >= entry.Offset + entry.ContentsLayout.Size)
                        {
                            currentLayout = null;
                            break;
                        }

                        // We found the field that contains the offset. Dive into this field.
                        path.Add(entry);
                        currentLayout = entry.ContentsLayout;
                        offset -= entry.Offset;
                        
                        stop = false;
                        break;
                    }
                }
            }

            return path[path.Count - 1].Offset == offset;
        } 
    }
}