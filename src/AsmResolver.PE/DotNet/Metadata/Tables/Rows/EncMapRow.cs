using System;
using System.Collections;
using System.Collections.Generic;

namespace AsmResolver.PE.DotNet.Metadata.Tables.Rows
{
    /// <summary>
    /// Represents a single row in the Edit-and-Continue remap metadata table.
    /// </summary>
    public readonly struct EncMapRow : IMetadataRow
    {
        /// <summary>
        /// Reads a single edit-and-continue remap row from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="layout">The layout of the edit-and-continue remap table.</param>
        /// <returns>The row.</returns>
        public static EncMapRow FromReader(IBinaryStreamReader reader, TableLayout layout)
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
        /// Gets the token that was remapped.
        /// </summary>
        public MetadataToken Token
        {
            get;
        }

        /// <summary>
        /// Determines whether this row is considered equal to the provided edit-and-continue remap row.
        /// </summary>
        /// <param name="other">The other row.</param>
        /// <returns><c>true</c> if the rows are equal, <c>false</c> otherwise.</returns>
        public bool Equals(EncMapRow other)
        {
            return Token.Equals(other.Token);
        }

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer, TableLayout layout)
        {
            writer.WriteUInt32(Token.ToUInt32());
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
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
    }
}