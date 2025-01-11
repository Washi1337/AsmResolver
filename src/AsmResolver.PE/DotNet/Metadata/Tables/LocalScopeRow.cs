using System;
using System.Collections;
using System.Collections.Generic;
using AsmResolver.IO;

namespace AsmResolver.PE.DotNet.Metadata.Tables
{
    /// <summary>
    /// Represents a single row in the Portable PDB local scope metadata table.
    /// </summary>
    public struct LocalScopeRow : IMetadataRow, IEquatable<LocalScopeRow>
    {
        /// <summary>
        /// Creates a new row for the Portable PDB Local Scope metadata table.
        /// </summary>
        /// <param name="method">An index into the method table that defines the scope.</param>
        /// <param name="importScope">An index into the import scope table that defines the scope.</param>
        /// <param name="variableList">An index into the local variable table referencing the first local variable in the method.</param>
        /// <param name="constantList">An index into the local constant table referencing the first constant in the method.</param>
        /// <param name="startOffset">The starting CIL offset of the scope.</param>
        /// <param name="length">The number of CIL bytes the scope spans.</param>
        public LocalScopeRow(uint method, uint importScope, uint variableList, uint constantList, uint startOffset, uint length)
        {
            Method = method;
            ImportScope = importScope;
            VariableList = variableList;
            ConstantList = constantList;
            StartOffset = startOffset;
            Length = length;
        }

        /// <inheritdoc />
        public TableIndex TableIndex => TableIndex.Document;

        /// <inheritdoc />
        public int Count => 6;

        /// <inheritdoc />
        public uint this[int index] => index switch
        {
            0 => Method,
            1 => ImportScope,
            2 => VariableList,
            3 => ConstantList,
            4 => StartOffset,
            5 => Length,
            _ => throw new IndexOutOfRangeException()
        };

        /// <summary>
        /// Gets or sets an index into the method table that defines the scope.
        /// </summary>
        public uint Method
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets an index into the import scope table that defines the scope.
        /// </summary>
        public uint ImportScope
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets an index into the local variable table referencing the first local variable in the method.
        /// </summary>
        public uint VariableList
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets an index into the local constant table referencing the first constant in the method.
        /// </summary>
        public uint ConstantList
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets The starting CIL offset of the scope.
        /// </summary>
        public uint StartOffset
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the number of CIL bytes the scope spans.
        /// </summary>
        public uint Length
        {
            get;
            set;
        }

        /// <summary>
        /// Reads a single Portable PDB local scope row from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="layout">The layout of the local socpe table.</param>
        /// <returns>The row.</returns>
        public static LocalScopeRow FromReader(ref BinaryStreamReader reader, TableLayout layout)
        {
            return new LocalScopeRow(
                reader.ReadIndex((IndexSize) layout.Columns[0].Size),
                reader.ReadIndex((IndexSize) layout.Columns[1].Size),
                reader.ReadIndex((IndexSize) layout.Columns[2].Size),
                reader.ReadIndex((IndexSize) layout.Columns[3].Size),
                reader.ReadUInt32(),
                reader.ReadUInt32());
        }

        /// <inheritdoc />
        public void Write(BinaryStreamWriter writer, TableLayout layout)
        {
            writer.WriteIndex(Method, (IndexSize) layout.Columns[0].Size);
            writer.WriteIndex(ImportScope, (IndexSize) layout.Columns[1].Size);
            writer.WriteIndex(VariableList, (IndexSize) layout.Columns[2].Size);
            writer.WriteIndex(ConstantList, (IndexSize) layout.Columns[3].Size);
            writer.WriteUInt32(StartOffset);
            writer.WriteUInt32(Length);
        }

        /// <inheritdoc />
        public bool Equals(LocalScopeRow other)
        {
            return Method == other.Method
                   && ImportScope == other.ImportScope
                   && VariableList == other.VariableList
                   && ConstantList == other.ConstantList
                   && StartOffset == other.StartOffset
                   && Length == other.Length;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is LocalScopeRow other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (int) Method;
                hashCode = (hashCode * 397) ^ (int) ImportScope;
                hashCode = (hashCode * 397) ^ (int) VariableList;
                hashCode = (hashCode * 397) ^ (int) ConstantList;
                hashCode = (hashCode * 397) ^ (int) StartOffset;
                hashCode = (hashCode * 397) ^ (int) Length;
                return hashCode;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"({Method:X8}, {ImportScope:X8}, {VariableList:X8}, {ConstantList:X8}, {StartOffset:X8}, {Length:X8})";
        }

        /// <inheritdoc />
        public IEnumerator<uint> GetEnumerator() => new MetadataRowColumnEnumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Determines whether two rows are considered equal.
        /// </summary>
        public static bool operator ==(LocalScopeRow left, LocalScopeRow right) => left.Equals(right);

        /// <summary>
        /// Determines whether two rows are not considered equal.
        /// </summary>
        public static bool operator !=(LocalScopeRow left, LocalScopeRow right) => !(left == right);

    }
}
