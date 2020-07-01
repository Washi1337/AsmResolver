using System;
using System.Collections.Generic;

namespace AsmResolver.DotNet.Memory
{
    /// <summary>
    /// Provides information about the memory layout of a type.
    /// </summary>
    public class TypeMemoryLayout
    {
        private readonly IDictionary<FieldDefinition, FieldMemoryLayout> _fields =
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
    }
}