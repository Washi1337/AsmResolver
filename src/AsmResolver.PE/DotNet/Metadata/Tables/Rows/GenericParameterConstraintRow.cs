using System;
using System.Collections;
using System.Collections.Generic;

namespace AsmResolver.PE.DotNet.Metadata.Tables.Rows
{
    /// <summary>
    /// Represents a single row in the generic parameter metadata table.
    /// </summary>
    public readonly struct GenericParameterConstraintRow : IMetadataRow
    {
        /// <summary>
        /// Reads a single generic parameter row from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="layout">The layout of the generic parameter table.</param>
        /// <returns>The row.</returns>
        public static GenericParameterConstraintRow FromReader(IBinaryStreamReader reader, TableLayout layout)
        {
            return new GenericParameterConstraintRow(
                reader.ReadIndex((IndexSize) layout.Columns[0].Size),
                reader.ReadIndex((IndexSize) layout.Columns[1].Size));
        }

        /// <summary>
        /// Creates anew row for the generic parameter constraint metadata table.
        /// </summary>
        /// <param name="owner">The index into the GenericParam table indicating the owner of the constraint.</param>
        /// <param name="constraint">The TypeDefOrRef index (an index into either the TypeRef, TypeDef or TypeSpec table)
        /// indicating the constraint that was put on the generic parameter.</param>
        public GenericParameterConstraintRow(uint owner, uint constraint)
        {
            Owner = owner;
            Constraint = constraint;
        }

        /// <inheritdoc />
        public TableIndex TableIndex => TableIndex.GenericParamConstraint;

        /// <inheritdoc />
        public int Count => 2;

        /// <inheritdoc />
        public uint this[int index] => index switch
        {
            0 => Owner,
            1 => Constraint,
            _ => throw new IndexOutOfRangeException()
        };

        /// <summary>
        /// Gets an index into the GenericParam table indicating the owner of the constraint.
        /// </summary>
        public uint Owner
        {
            get;
        }

        /// <summary>
        /// Gets a TypeDefOrRef index (an index into either the TypeRef, TypeDef or TypeSpec table) indicating the
        /// constraint that was put on the generic parameter.
        /// </summary>
        public uint Constraint
        {
            get;
        }

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer, TableLayout layout)
        {
            writer.WriteIndex(Owner, (IndexSize) layout.Columns[0].Size);
            writer.WriteIndex(Constraint, (IndexSize) layout.Columns[1].Size);
        }

        /// <summary>
        /// Determines whether this row is considered equal to the provided generic parameter constraint row.
        /// </summary>
        /// <param name="other">The other row.</param>
        /// <returns><c>true</c> if the rows are equal, <c>false</c> otherwise.</returns>
        public bool Equals(GenericParameterConstraintRow other)
        {
            return Owner == other.Owner && Constraint == other.Constraint;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is GenericParameterConstraintRow other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) Owner * 397) ^ (int) Constraint;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"({Owner:X8}, {Constraint:X8})";
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