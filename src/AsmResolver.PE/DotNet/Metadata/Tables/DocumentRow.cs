using System;
using System.Collections;
using System.Collections.Generic;
using AsmResolver.IO;

namespace AsmResolver.PE.DotNet.Metadata.Tables
{
    /// <summary>
    /// Represents a single row in the Portable PDB Document metadata table.
    /// </summary>
    public struct DocumentRow : IMetadataRow
    {
        /// <summary>
        /// Creates a new row for the Portable PDB Document metadata table.
        /// </summary>
        /// <param name="name">The index into the blob stream referencing the name of the document.</param>
        /// <param name="hashAlgorithm">The index into the GUID stream referencing the hash algorithm identifier.</param>
        /// <param name="hash">The index into the blob stream referencing the hash of the document.</param>
        /// <param name="language">The index into the GUID stream referencing the language identifier.</param>
        public DocumentRow(uint name, uint hashAlgorithm, uint hash, uint language)
        {
            Name = name;
            HashAlgorithm = hashAlgorithm;
            Hash = hash;
            Language = language;
        }

        /// <inheritdoc />
        public TableIndex TableIndex => TableIndex.Document;

        /// <inheritdoc />
        public int Count => 4;

        /// <inheritdoc />
        public uint this[int index] => index switch
        {
            0 => Name,
            1 => HashAlgorithm,
            2 => Hash,
            3 => Language,
            _ => throw new IndexOutOfRangeException()
        };

        /// <summary>
        /// Gets or sets an index into the blob stream referencing the name of the document.
        /// </summary>
        public uint Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets an index into the GUID stream referencing the hash algorithm identifier.
        /// </summary>
        public uint HashAlgorithm
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets an index into the blob stream referencing the hash of the document.
        /// </summary>
        public uint Hash
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets an index into the GUID stream referencing the language identifier.
        /// </summary>
        public uint Language
        {
            get;
            set;
        }

        /// <summary>
        /// Reads a single Portable PDB Document row from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="layout">The layout of the document table.</param>
        /// <returns>The row.</returns>
        public static DocumentRow FromReader(ref BinaryStreamReader reader, TableLayout layout)
        {
            return new DocumentRow(
                reader.ReadIndex((IndexSize) layout.Columns[0].Size),
                reader.ReadIndex((IndexSize) layout.Columns[1].Size),
                reader.ReadIndex((IndexSize) layout.Columns[2].Size),
                reader.ReadIndex((IndexSize) layout.Columns[3].Size));
        }

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer, TableLayout layout)
        {
            writer.WriteIndex(Name, (IndexSize) layout.Columns[0].Size);
            writer.WriteIndex(HashAlgorithm, (IndexSize) layout.Columns[1].Size);
            writer.WriteIndex(Hash, (IndexSize) layout.Columns[2].Size);
            writer.WriteIndex(Language, (IndexSize) layout.Columns[3].Size);
        }

        /// <summary>
        /// Determines whether this row is considered equal to the provided document row.
        /// </summary>
        /// <param name="other">The other row.</param>
        /// <returns><c>true</c> if the rows are equal, <c>false</c> otherwise.</returns>
        public bool Equals(DocumentRow other)
        {
            return Name == other.Name
                   && HashAlgorithm == other.HashAlgorithm
                   && Hash == other.Hash
                   && Language == other.Language;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is DocumentRow other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (int) Name;
                hashCode = (hashCode * 397) ^ (int) HashAlgorithm;
                hashCode = (hashCode * 397) ^ (int) Hash;
                hashCode = (hashCode * 397) ^ (int) Language;
                return hashCode;
            }
        }

        /// <inheritdoc />
        public override string ToString() => $"({Name:X8}, {HashAlgorithm:X8}, {Hash:X8}, {Language:X8})";

        /// <inheritdoc />
        public IEnumerator<uint> GetEnumerator() => new MetadataRowColumnEnumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
