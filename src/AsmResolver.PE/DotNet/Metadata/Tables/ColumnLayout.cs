using System;

namespace AsmResolver.PE.DotNet.Metadata.Tables
{
    /// <summary>
    /// Provides a description of a single column in a metadata table. 
    /// </summary>
    public class ColumnLayout
    {
        /// <summary>
        /// Defines a new column layout, using a fixed size column type.
        /// </summary>
        /// <param name="name">The name of the column.</param>
        /// <param name="type">The value type of the column.</param>
        /// <exception cref="ArgumentException">Occurs when the provided column type does not have a fixed size.</exception>
        public ColumnLayout(string name, ColumnType type)
        {
            Name = name;
            Type = type;
            if (type < ColumnType.Byte)
                throw new ArgumentException("Size was not explicitly defined.");
            Size = (uint) type & 0xFF;
        }

        /// <summary>
        /// Defines a new column layout, using a fixed size column type.
        /// </summary>
        /// <param name="name">The name of the column.</param>
        /// <param name="type">The value type of the column.</param>
        /// <param name="size">The size of the column.</param>
        public ColumnLayout(string name, ColumnType type, IndexSize size)
            : this(name, type, (uint) size)
        {
        }

        /// <summary>
        /// Defines a new column layout, using a fixed size column type.
        /// </summary>
        /// <param name="name">The name of the column.</param>
        /// <param name="type">The value type of the column.</param>
        /// <param name="size">The size of the column.</param>
        public ColumnLayout(string name, ColumnType type, uint size)
        {
            Name = name;
            Type = type;
            Size = size;
        }
        
        /// <summary>
        /// Gets the name of the column.
        /// </summary>
        public string Name
        {
            get;
        }
        
        /// <summary>
        /// Gets the data type that this column persists.
        /// </summary>
        public ColumnType Type
        {
            get;
        }

        /// <summary>
        /// Gets the size in bytes of each cell in this column.
        /// </summary>
        public uint Size
        {
            get;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Name} : {Type} ({Size} bytes)";
        }
    }
}