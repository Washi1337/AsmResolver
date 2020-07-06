using System;
using System.Collections;
using System.Collections.Generic;

namespace AsmResolver.PE.DotNet.Metadata.Tables.Rows
{
    /// <summary>
    /// Represents a single row in the class layout metadata table.
    /// </summary>
    public readonly struct ClassLayoutRow : IMetadataRow
    {
        /// <summary>
        /// Reads a single class layout row from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="layout">The layout of the class layout table.</param>
        /// <returns>The row.</returns>
        public static ClassLayoutRow FromReader(IBinaryStreamReader reader, TableLayout layout)
        {
            return new ClassLayoutRow(
                reader.ReadUInt16(),
                reader.ReadUInt32(),
                reader.ReadIndex((IndexSize) layout.Columns[2].Size));
        }
        
        /// <summary>
        /// Creates a new row for the class layout metadata table,
        /// </summary>
        /// <param name="packingSize">The alignment in bytes of each field in the type. </param>
        /// <param name="classSize">The size in bytes of the type.</param>
        /// <param name="parent">The index into the TypeDef table indicating the type that this layout is assigned to.</param>
        public ClassLayoutRow(ushort packingSize, uint classSize, uint parent)
        {
            PackingSize = packingSize;
            ClassSize = classSize;
            Parent = parent;
        }

        /// <inheritdoc />
        public TableIndex TableIndex => TableIndex.ClassLayout;

        /// <inheritdoc />
        public int Count => 3;

        /// <inheritdoc />
        public uint this[int index] => index switch
        {
            0 => PackingSize,
            1 => ClassSize,
            2 => Parent,
            _ => throw new IndexOutOfRangeException()
        };

        /// <summary>
        /// Gets the alignment in bytes of each field in the type. 
        /// </summary>
        /// <remarks>
        /// This value should be a power of two between 0 and 128.
        /// </remarks>
        public ushort PackingSize
        {
            get;
        }

        /// <summary>
        /// Gets the size in bytes of the type.
        /// </summary>
        public uint ClassSize
        {
            get;
        }

        /// <summary>
        /// Gets an index into the TypeDef table indicating the type that this layout is assigned to.
        /// </summary>
        public uint Parent
        {
            get;
        }

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer, TableLayout layout)
        {
            writer.WriteUInt16(PackingSize);
            writer.WriteUInt32(ClassSize);
            writer.WriteIndex(Parent, (IndexSize) layout.Columns[2].Size);
        }

        /// <summary>
        /// Determines whether this row is considered equal to the provided class layout row.
        /// </summary>
        /// <param name="other">The other row.</param>
        /// <returns><c>true</c> if the rows are equal, <c>false</c> otherwise.</returns>
        public bool Equals(ClassLayoutRow other)
        {
            return PackingSize == other.PackingSize && ClassSize == other.ClassSize && Parent == other.Parent;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is ClassLayoutRow other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = PackingSize.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) ClassSize;
                hashCode = (hashCode * 397) ^ (int) Parent;
                return hashCode;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"({PackingSize:X4}, {ClassSize:X8}, {Parent:X8})";
        }

        /// <inheritdoc />
        public IEnumerator<uint> GetEnumerator()
        {
            return new MetadataRowColumnEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}