using System;

using System.Collections;
using System.Collections.Generic;
using AsmResolver.IO;

namespace AsmResolver.PE.DotNet.Metadata.Tables
{
    /// <summary>
    /// Represents a single row in the Edit-and-Continue log metadata table.
    /// </summary>
    public struct EncLogRow : IMetadataRow, IEquatable<EncLogRow>
    {
        /// <summary>
        /// Reads a single edit-and-continue log row from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="layout">The layout of the edit-and-continue log table.</param>
        /// <returns>The row.</returns>
        public static EncLogRow FromReader(ref BinaryStreamReader reader, TableLayout layout)
        {
            return new EncLogRow(
                reader.ReadUInt32(),
                (DeltaFunctionCode) reader.ReadUInt32());
        }

        /// <summary>
        /// Creates a new row for the edit-and-continue log metadata table.
        /// </summary>
        /// <param name="token">The metadata token to apply the delta function to.</param>
        /// <param name="funcCode">The delta function to apply.</param>
        public EncLogRow(MetadataToken token, DeltaFunctionCode funcCode)
        {
            Token = token;
            FuncCode = funcCode;
        }

        /// <inheritdoc />
        public TableIndex TableIndex => TableIndex.EncLog;

        /// <inheritdoc />
        public int Count => 2;

        /// <inheritdoc />
        public uint this[int index] => index switch
        {
            0 => Token.ToUInt32(),
            1 => (uint) FuncCode,
            _ => throw new IndexOutOfRangeException()
        };

        /// <summary>
        /// Gets or sets the metadata token to apply the delta function to.
        /// </summary>
        public MetadataToken Token
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the delta function to apply.
        /// </summary>
        public DeltaFunctionCode FuncCode
        {
            get;
            set;
        }

        /// <inheritdoc />
        public bool Equals(EncLogRow other)
        {
            return Token.Equals(other.Token) && FuncCode == other.FuncCode;
        }

        /// <inheritdoc />
        public void Write(BinaryStreamWriter writer, TableLayout layout)
        {
            writer.WriteUInt32(Token.ToUInt32());
            writer.WriteUInt32((uint) FuncCode);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is EncLogRow other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (Token.GetHashCode() * 397) ^ (int) FuncCode;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"({Token}, {(int) FuncCode:X8})";
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
        public static bool operator ==(EncLogRow left, EncLogRow right) => left.Equals(right);

        /// <summary>
        /// Determines whether two rows are not considered equal.
        /// </summary>
        public static bool operator !=(EncLogRow left, EncLogRow right) => !(left == right);
    }
}
