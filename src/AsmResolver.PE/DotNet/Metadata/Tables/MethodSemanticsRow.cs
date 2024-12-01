using System;
using System.Collections;
using System.Collections.Generic;
using AsmResolver.IO;

namespace AsmResolver.PE.DotNet.Metadata.Tables
{
    /// <summary>
    /// Represents a single row in the method semantics metadata table.
    /// </summary>
    public struct MethodSemanticsRow : IMetadataRow, IEquatable<MethodSemanticsRow>
    {
        /// <summary>
        /// Reads a single method semantics row from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="layout">The layout of the method semantics table.</param>
        /// <returns>The row.</returns>
        public static MethodSemanticsRow FromReader(ref BinaryStreamReader reader, TableLayout layout)
        {
            return new MethodSemanticsRow(
                (MethodSemanticsAttributes) reader.ReadUInt16(),
                reader.ReadIndex((IndexSize) layout.Columns[1].Size),
                reader.ReadIndex((IndexSize) layout.Columns[2].Size));
        }

        /// <summary>
        /// Creates a new row for the method semantics table.
        /// </summary>
        /// <param name="attributes">The semantic attributes that are assigned to the method.</param>
        /// <param name="method">The index into the method definition table indicating the method that was assigned
        /// special semantics.</param>
        /// <param name="association">The index into the method definition table indicating the method that was assigned
        /// special semantics.</param>
        public MethodSemanticsRow(MethodSemanticsAttributes attributes, uint method, uint association)
        {
            Attributes = attributes;
            Method = method;
            Association = association;
        }

        /// <inheritdoc />
        public TableIndex TableIndex => TableIndex.MethodSemantics;

        /// <inheritdoc />
        public int Count => 3;

        /// <inheritdoc />
        public uint this[int index] => index switch
        {
            0 => (uint) Attributes,
            1 => Method,
            2 => Association,
            _ => throw new IndexOutOfRangeException()
        };

        /// <summary>
        /// Gets or sets the semantic attributes that are assigned to the method.
        /// </summary>
        public MethodSemanticsAttributes Attributes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets an index into the method definition table indicating the method that was assigned special semantics.
        /// </summary>
        public uint Method
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a HasSemantics index (an index into either the event or property table) indicating the member the method
        /// is associated with.
        /// </summary>
        public uint Association
        {
            get;
            set;
        }

        /// <inheritdoc />
        public void Write(BinaryStreamWriter writer, TableLayout layout)
        {
            writer.WriteUInt16((ushort) Attributes);
            writer.WriteIndex(Method, (IndexSize) layout.Columns[1].Size);
            writer.WriteIndex(Association, (IndexSize) layout.Columns[2].Size);
        }

        /// <inheritdoc />
        public bool Equals(MethodSemanticsRow other)
        {
            return Attributes == other.Attributes && Method == other.Method && Association == other.Association;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is MethodSemanticsRow other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (int) Attributes;
                hashCode = (hashCode * 397) ^ (int) Method;
                hashCode = (hashCode * 397) ^ (int) Association;
                return hashCode;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"({(int) Attributes:X4}, {Method:X8}, {Association:X8})";
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

        /// <summary>
        /// Determines whether two rows are considered equal.
        /// </summary>
        public static bool operator ==(MethodSemanticsRow left, MethodSemanticsRow right) => left.Equals(right);

        /// <summary>
        /// Determines whether two rows are not considered equal.
        /// </summary>
        public static bool operator !=(MethodSemanticsRow left, MethodSemanticsRow right) => !(left == right);
    }
}
