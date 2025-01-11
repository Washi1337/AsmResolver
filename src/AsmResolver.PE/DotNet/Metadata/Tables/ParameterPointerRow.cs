using System;
using System.Collections;
using System.Collections.Generic;
using AsmResolver.IO;

namespace AsmResolver.PE.DotNet.Metadata.Tables
{
    /// <summary>
    /// Represents a single row in the parameter pointer metadata table.
    /// </summary>
    public struct ParameterPointerRow : IMetadataRow, IEquatable<ParameterPointerRow>
    {
        /// <summary>
        /// Reads a single parameter pointer row from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="layout">The layout of the parameter pointer table.</param>
        /// <returns>The row.</returns>
        public static ParameterPointerRow FromReader(ref BinaryStreamReader reader, TableLayout layout)
        {
            return new ParameterPointerRow(reader.ReadIndex((IndexSize) layout.Columns[0].Size));
        }

        /// <summary>
        /// Creates a new row for the parameter pointer metadata table.
        /// </summary>
        /// <param name="parameter">The index into the Parameter table that this pointer references.</param>
        public ParameterPointerRow(uint parameter)
        {
            Parameter = parameter;
        }

        /// <inheritdoc />
        public TableIndex TableIndex => TableIndex.ParamPtr;

        /// <inheritdoc />
        public int Count => 1;

        /// <inheritdoc />
        public uint this[int index] => index switch
        {
            0 => Parameter,
            _ => throw new IndexOutOfRangeException()
        };

        /// <summary>
        /// Gets or sets an index into the Parameter table that this pointer references.
        /// </summary>
        public uint Parameter
        {
            get;
            set;
        }

        /// <inheritdoc />
        public void Write(BinaryStreamWriter writer, TableLayout layout)
        {
            writer.WriteIndex(Parameter, (IndexSize) layout.Columns[0].Size);
        }

        /// <inheritdoc />
        public bool Equals(ParameterPointerRow other)
        {
            return Parameter == other.Parameter;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is ParameterPointerRow other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return (int) Parameter;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"({Parameter:X8})";
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
        public static bool operator ==(ParameterPointerRow left, ParameterPointerRow right) => left.Equals(right);

        /// <summary>
        /// Determines whether two rows are not considered equal.
        /// </summary>
        public static bool operator !=(ParameterPointerRow left, ParameterPointerRow right) => !(left == right);
    }
}
