using System;
using System.Collections;
using System.Collections.Generic;

namespace AsmResolver.PE.DotNet.Metadata.Tables.Rows
{
    /// <summary>
    /// Represents a single row in the constants metadata table.
    /// </summary>
    public readonly struct ConstantRow : IMetadataRow
    {
        /// <summary>
        /// Reads a single constant row from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="layout">The layout of the constants table.</param>
        /// <returns>The row.</returns>
        public static ConstantRow FromReader(IBinaryStreamReader reader, TableLayout layout)
        {
            return new ConstantRow(
                (ElementType) reader.ReadByte(),
                reader.ReadByte(),
                reader.ReadIndex((IndexSize) layout.Columns[2].Size),
                reader.ReadIndex((IndexSize) layout.Columns[3].Size));
        }

        /// <summary>
        /// Creates a new row for the constants metadata table.
        /// </summary>
        /// <param name="type">The type of constant that is stored in the blob stream. </param>
        /// <param name="parent">The HasConstant index (an index into either the Field, Parameter or Property table)
        /// that is the owner of the constant.</param>
        /// <param name="value">The index into the #Blob stream containing the serialized constant value.</param>
        public ConstantRow(ElementType type, uint parent, uint value)
        {
            Type = type;
            Padding = 0;
            Parent = parent;
            Value = value;
        }
        
        /// <summary>
        /// Creates a new row for the constants metadata table.
        /// </summary>
        /// <param name="type">The type of constant that is stored in the blob stream. </param>
        /// <param name="padding">The single padding byte between the type and parent columns.</param>
        /// <param name="parent">The HasConstant index (an index into either the Field, Parameter or Property table)
        /// that is the owner of the constant.</param>
        /// <param name="value">The index into the #Blob stream containing the serialized constant value.</param>
        public ConstantRow(ElementType type, byte padding, uint parent, uint value)
        {
            Type = type;
            Padding = padding;
            Parent = parent;
            Value = value;
        }

        /// <inheritdoc />
        public TableIndex TableIndex => TableIndex.Constant;

        /// <inheritdoc />
        public int Count => 4;

        /// <inheritdoc />
        public uint this[int index] => index switch
        {
            0 => (uint) Type,
            1 => Padding,
            2 => Parent,
            3 => Value,
            _ => throw new IndexOutOfRangeException()
        };
        
        /// <summary>
        /// Gets the type of constant that is stored in the blob stream. 
        /// </summary>
        /// <remarks>This field must always be a value-type.</remarks>
        public ElementType Type
        {
            get;
        }

        /// <summary>
        /// Gets the single padding byte between the type and parent columns.
        /// </summary>
        /// <remarks>This field should always be zero.</remarks>
        public byte Padding
        {
            get;
        }

        /// <summary>
        /// Gets a HasConstant index (an index into either the Field, Parameter or Property table) that is the owner
        /// of the constant. 
        /// </summary>
        public uint Parent
        {
            get;
        }

        /// <summary>
        /// Gets an index into the #Blob stream containing the serialized constant value.
        /// </summary>
        public uint Value
        {
            get;
        }

        /// <summary>
        /// Determines whether this row is considered equal to the provided constant row.
        /// </summary>
        /// <param name="other">The other row.</param>
        /// <returns><c>true</c> if the rows are equal, <c>false</c> otherwise.</returns>
        public bool Equals(ConstantRow other)
        {
            return Type == other.Type
                   && Padding == other.Padding
                   && Parent == other.Parent
                   && Value == other.Value;
        }

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer, TableLayout layout)
        {
            writer.WriteByte((byte) Type);
            writer.WriteByte(Padding);
            writer.WriteIndex(Parent, (IndexSize) layout.Columns[2].Size);
            writer.WriteIndex(Value, (IndexSize) layout.Columns[3].Size);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is ConstantRow other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (int) Type;
                hashCode = (hashCode * 397) ^ Padding.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) Parent;
                hashCode = (hashCode * 397) ^ (int) Value;
                return hashCode;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"({(int) Type:X2}, {Padding:X2}, {Parent:X8}, {Value:X8})";
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
