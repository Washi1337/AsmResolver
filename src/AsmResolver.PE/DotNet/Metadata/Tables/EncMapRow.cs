using System;
using System.Collections;
using System.Collections.Generic;
using AsmResolver.IO;

namespace AsmResolver.PE.DotNet.Metadata.Tables
{
    /// <summary>
    /// Represents a single row in the Edit-and-Continue remap metadata table.
    /// </summary>
    public struct EncMapRow : IMetadataRow, IEquatable<EncMapRow>
    {
        /// <summary>
        /// Reads a single edit-and-continue remap row from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="layout">The layout of the edit-and-continue remap table.</param>
        /// <returns>The row.</returns>
        public static EncMapRow FromReader(ref BinaryStreamReader reader, TableLayout layout)
        {
            return new EncMapRow(reader.ReadUInt32());
        }

        /// <summary>
        /// Creates a new row for the edit-and-continue remap metadata table.
        /// </summary>
        /// <param name="token">The token that was remapped.</param>
        public EncMapRow(MetadataToken token)
        {
            Token = token;
        }

        /// <inheritdoc />
        public TableIndex TableIndex => TableIndex.EncMap;

        /// <inheritdoc />
        public int Count => 1;

        /// <inheritdoc />
        public uint this[int index] => index switch
        {
            0 => Token.ToUInt32(),
            _ => throw new IndexOutOfRangeException()
        };

        /// <summary>
        /// Gets or sets the token that was remapped.
        /// </summary>
        public MetadataToken Token
        {
            get;
            set;
        }

        /// <inheritdoc />
        public bool Equals(EncMapRow other)
        {
            return Token.Equals(other.Token);
        }

        /// <inheritdoc />
        public void Write(BinaryStreamWriter writer, TableLayout layout)
        {
            writer.WriteUInt32(Token.ToUInt32());
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is EncMapRow other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Token.GetHashCode();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"({Token})";
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
        public static bool operator ==(EncMapRow left, EncMapRow right) => left.Equals(right);

        /// <summary>
        /// Determines whether two rows are not considered equal.
        /// </summary>
        public static bool operator !=(EncMapRow left, EncMapRow right) => !(left == right);
    }
}
