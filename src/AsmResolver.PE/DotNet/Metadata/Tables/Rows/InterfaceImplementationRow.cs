using System;
using System.Collections;
using System.Collections.Generic;

namespace AsmResolver.PE.DotNet.Metadata.Tables.Rows
{
    /// <summary>
    /// Represents a single row in the interface implementation metadata table.
    /// </summary>
    public readonly struct InterfaceImplementationRow : IMetadataRow
    {
        /// <summary>
        /// Reads a single interface implementation row from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="layout">The layout of the interface implementation table.</param>
        /// <returns>The row.</returns>
        public static InterfaceImplementationRow FromReader(IBinaryStreamReader reader, TableLayout layout)
        {
            return new InterfaceImplementationRow(
                reader.ReadIndex((IndexSize) layout.Columns[0].Size),
                reader.ReadIndex((IndexSize) layout.Columns[1].Size));
        }
        
        /// <summary>
        /// Creates a new row for the interface implementation metadata table.
        /// </summary>
        /// <param name="class">The index into the TypeDef table indicating the type that implements the interface.</param>
        /// <param name="interface">The TypeDefOrRef (an index to a row in either the TypeRef, TypeDef or TypeSpec table)
        /// indicating the interface that was implemented by the type.</param>
        public InterfaceImplementationRow(uint @class, uint @interface)
        {
            Class = @class;
            Interface = @interface;
        }

        /// <inheritdoc />
        public TableIndex TableIndex => TableIndex.InterfaceImpl;

        /// <inheritdoc />
        public int Count => 2;

        /// <inheritdoc />
        public uint this[int index] => index switch
        {
            0 => Class,
            1 => Interface,
            _ => throw new IndexOutOfRangeException()
        };

        /// <summary>
        /// Gets an index into the TypeDef table indicating the type that implements the interface.
        /// </summary>
        public uint Class
        {
            get;
        }

        /// <summary>
        /// Gets a TypeDefOrRef (an index to a row in either the TypeRef, TypeDef or TypeSpec table) indicating the
        /// interface that was implemented by the type.
        /// </summary>
        public uint Interface
        {
            get;
        }

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer, TableLayout layout)
        {
            writer.WriteIndex(Class, (IndexSize) layout.Columns[0].Size);
            writer.WriteIndex(Interface, (IndexSize) layout.Columns[1].Size);
        }

        /// <summary>
        /// Determines whether this row is considered equal to the provided interface implementation row.
        /// </summary>
        /// <param name="other">The other row.</param>
        /// <returns><c>true</c> if the rows are equal, <c>false</c> otherwise.</returns>
        public bool Equals(InterfaceImplementationRow other)
        {
            return Class == other.Class && Interface == other.Interface;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is InterfaceImplementationRow other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) Class * 397) ^ (int) Interface;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"({Class:X8}, {Interface:X8})";
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