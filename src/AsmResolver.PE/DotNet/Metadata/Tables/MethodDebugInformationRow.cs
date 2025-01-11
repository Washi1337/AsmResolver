using System;
using System.Collections;
using System.Collections.Generic;
using AsmResolver.IO;

namespace AsmResolver.PE.DotNet.Metadata.Tables
{
    /// <summary>
    /// Represents a single row in the Portable PDB Document metadata table.
    /// </summary>
    public struct MethodDebugInformationRow : IMetadataRow, IEquatable<MethodDebugInformationRow>
    {
        /// <summary>
        /// Creates a new row for the Portable PDB Method Debug Information metadata table.
        /// </summary>
        /// <param name="document">
        /// The index into the Document table referencing the document that declares the method.
        /// </param>
        /// <param name="sequencePoints">
        /// The index into the blob stream referencing an array of sequence points that make up the method.
        /// </param>
        public MethodDebugInformationRow(uint document, uint sequencePoints)
        {
            Document = document;
            SequencePoints = sequencePoints;
        }

        /// <inheritdoc />
        public TableIndex TableIndex => TableIndex.Document;

        /// <inheritdoc />
        public int Count => 2;

        /// <inheritdoc />
        public uint this[int index] => index switch
        {
            0 => Document,
            1 => SequencePoints,
            _ => throw new IndexOutOfRangeException()
        };

        /// <summary>
        /// Gets or sets an index into the Document table referencing the document that declares the method, or 0
        /// if the method does not have sequence points or spans multiple documents.
        /// </summary>
        public uint Document
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets an index into the blob stream referencing an array of sequence points that make up the method,
        /// or 0 if no sequence points are available.
        /// </summary>
        public uint SequencePoints
        {
            get;
            set;
        }

        /// <summary>
        /// Reads a single Portable PDB Method Debug Information row from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="layout">The layout of the method debug information table.</param>
        /// <returns>The row.</returns>
        public static MethodDebugInformationRow FromReader(ref BinaryStreamReader reader, TableLayout layout)
        {
            return new MethodDebugInformationRow(
                reader.ReadIndex((IndexSize) layout.Columns[0].Size),
                reader.ReadIndex((IndexSize) layout.Columns[1].Size));
        }

        /// <inheritdoc />
        public void Write(BinaryStreamWriter writer, TableLayout layout)
        {
            writer.WriteIndex(Document, (IndexSize) layout.Columns[0].Size);
            writer.WriteIndex(SequencePoints, (IndexSize) layout.Columns[1].Size);
        }

        /// <inheritdoc />
        public bool Equals(MethodDebugInformationRow other)
        {
            return Document == other.Document && SequencePoints == other.SequencePoints;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is MethodDebugInformationRow other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) Document * 397) ^ (int) SequencePoints;
            }
        }

        /// <inheritdoc />
        public override string ToString() => $"({Document:X8}, {SequencePoints:X8})";

        /// <inheritdoc />
        public IEnumerator<uint> GetEnumerator()
        {
            return new MetadataRowColumnEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Determines whether two rows are considered equal.
        /// </summary>
        public static bool operator ==(MethodDebugInformationRow left, MethodDebugInformationRow right) => left.Equals(right);

        /// <summary>
        /// Determines whether two rows are not considered equal.
        /// </summary>
        public static bool operator !=(MethodDebugInformationRow left, MethodDebugInformationRow right) => !(left == right);
    }
}
