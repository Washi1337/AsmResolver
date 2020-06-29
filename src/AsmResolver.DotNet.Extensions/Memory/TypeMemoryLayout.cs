using System.Collections.Generic;

namespace AsmResolver.DotNet.Memory
{
    /// <summary>
    /// Provides information about the memory layout of a type.
    /// </summary>
    public class TypeMemoryLayout
    {
        /// <summary>
        /// Creates a new instance of the <see cref="TypeMemoryLayout"/> class.
        /// </summary>
        /// <param name="size">The size of the type.</param>
        public TypeMemoryLayout(int size)
        {
            Size = size;
            Fields = new Dictionary<FieldDefinition, FieldLayout>();
        }
        
        public int Size
        {
            get;
        }

        public IDictionary<FieldDefinition, FieldLayout> Fields
        {
            get;
        }
    }
}