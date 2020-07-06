using System;
using System.Collections;
using System.Collections.Generic;

namespace AsmResolver.PE.DotNet.Metadata.Tables.Rows
{
    /// <summary>
    /// Represents a single row in the file metadata table.
    /// </summary>
    public readonly struct FileReferenceRow : IMetadataRow
    {
        /// <summary>
        /// Reads a single file row from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="layout">The layout of the file table.</param>
        /// <returns>The row.</returns>
        public static FileReferenceRow FromReader(IBinaryStreamReader reader, TableLayout layout)
        {
            return new FileReferenceRow(
                (FileAttributes) reader.ReadUInt32(),
                reader.ReadIndex((IndexSize) layout.Columns[1].Size),
                reader.ReadIndex((IndexSize) layout.Columns[2].Size));
        }

        /// <summary>
        /// Creates a new row for the file reference metadata table.
        /// </summary>
        /// <param name="attributes">The attributes associated to the file reference.</param>
        /// <param name="name">The index into the #Strings stream referencing the name of the file.</param>
        /// <param name="hashValue">The  index into the #Blob stream referencing the hash value of the file.</param>
        public FileReferenceRow(FileAttributes attributes, uint name, uint hashValue)
        {
            Attributes = attributes;
            Name = name;
            HashValue = hashValue;
        }

        /// <inheritdoc />
        public TableIndex TableIndex => TableIndex.File;

        /// <inheritdoc />
        public int Count => 3;

        /// <inheritdoc />
        public uint this[int index] => index switch
        {
            0 => (uint) Attributes,
            1 => Name,
            2 => HashValue,
            _ => throw new IndexOutOfRangeException()
        };

        /// <summary>
        /// Gets the attributes associated to the file reference.
        /// </summary>
        public FileAttributes Attributes
        {
            get;
        }

        /// <summary>
        /// Gets an index into the #Strings stream referencing the name of the file.
        /// </summary>
        public uint Name
        {
            get;
        }

        /// <summary>
        /// Gets an index into the #Blob stream referencing the hash value of the file.
        /// </summary>
        public uint HashValue
        {
            get;
        }

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer, TableLayout layout)
        {
            writer.WriteUInt32((uint) Attributes);
            writer.WriteIndex(Name, (IndexSize) layout.Columns[1].Size);
            writer.WriteIndex(HashValue, (IndexSize) layout.Columns[2].Size);
        }

        /// <summary>
        /// Determines whether this row is considered equal to the provided file row.
        /// </summary>
        /// <param name="other">The other row.</param>
        /// <returns><c>true</c> if the rows are equal, <c>false</c> otherwise.</returns>
        public bool Equals(FileReferenceRow other)
        {
            return Attributes == other.Attributes && Name == other.Name && HashValue == other.HashValue;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is FileReferenceRow other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (int) Attributes;
                hashCode = (hashCode * 397) ^ (int) Name;
                hashCode = (hashCode * 397) ^ (int) HashValue;
                return hashCode;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"({(int) Attributes:X8}, {Name:X8}, {HashValue:X8})";
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