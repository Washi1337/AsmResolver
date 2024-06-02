using System;
using System.Collections;
using System.Collections.Generic;
using AsmResolver.IO;

namespace AsmResolver.PE.DotNet.Metadata.Tables
{
    /// <summary>
    /// Represents a single row in the Portable PDB state machine method metadata table.
    /// </summary>
    public struct StateMachineMethodRow : IMetadataRow
    {
        /// <summary>
        /// Creates a new row for the Portable PDB state machine method metadata table.
        /// </summary>
        /// <param name="moveNextMethod">
        /// An index into the method table referencing the MoveNext method of an async state machine.
        /// </param>
        /// <param name="kickoffMethod">
        /// An index into the method table referencing the kickoff method of an async state machine.
        /// </param>
        public StateMachineMethodRow(uint moveNextMethod, uint kickoffMethod)
        {
            MoveNextMethod = moveNextMethod;
            KickoffMethod = kickoffMethod;
        }

        /// <inheritdoc />
        public TableIndex TableIndex => TableIndex.StateMachineMethod;

        /// <inheritdoc />
        public int Count => 2;

        /// <inheritdoc />
        public uint this[int index] => index switch
        {
            0 => MoveNextMethod,
            1 => KickoffMethod,
            _ => throw new IndexOutOfRangeException()
        };

        /// <summary>
        /// Gets or sets an index into the method table referencing the MoveNext method of an async state machine.
        /// </summary>
        public uint MoveNextMethod
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets an index into the method table referencing the kickoff method of an async state machine.
        /// </summary>
        public uint KickoffMethod
        {
            get;
            set;
        }

        /// <summary>
        /// Reads a single Portable PDB state machine method row from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="layout">The layout of the state machine method table.</param>
        /// <returns>The row.</returns>
        public static StateMachineMethodRow FromReader(ref BinaryStreamReader reader, TableLayout layout)
        {
            return new StateMachineMethodRow(
                reader.ReadIndex((IndexSize) layout.Columns[0].Size),
                reader.ReadIndex((IndexSize) layout.Columns[1].Size));
        }

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer, TableLayout layout)
        {
            writer.WriteIndex(MoveNextMethod, (IndexSize) layout.Columns[0].Size);
            writer.WriteIndex(KickoffMethod, (IndexSize) layout.Columns[1].Size);
        }

        /// <summary>
        /// Determines whether this row is considered equal to the provided state machine method row.
        /// </summary>
        /// <param name="other">The other row.</param>
        /// <returns><c>true</c> if the rows are equal, <c>false</c> otherwise.</returns>
        public bool Equals(StateMachineMethodRow other)
        {
            return MoveNextMethod == other.MoveNextMethod && KickoffMethod == other.KickoffMethod;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is StateMachineMethodRow other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) MoveNextMethod * 397) ^ (int) KickoffMethod;
            }
        }

        /// <inheritdoc />
        public override string ToString() => $"({MoveNextMethod:X8}, {KickoffMethod:X8})";

        /// <inheritdoc />
        public IEnumerator<uint> GetEnumerator() => new MetadataRowColumnEnumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
