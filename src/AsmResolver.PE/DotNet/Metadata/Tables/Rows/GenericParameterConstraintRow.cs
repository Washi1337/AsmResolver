// AsmResolver - Executable file format inspection library 
// Copyright (C) 2016-2019 Washi
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA

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

        public GenericParameterConstraintRow(uint owner, uint constraint)
        {
            Owner = owner;
            Constraint = constraint;
        }

        /// <inheritdoc />
        public TableIndex TableIndex => TableIndex.GenericParamConstraint;

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
        
    }
}