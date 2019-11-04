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
    /// Represents a single row in the property map metadata table.
    /// </summary>
    public readonly struct PropertyMapRow : IMetadataRow
    { 
        /// <summary>
        /// Reads a single property map row from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="layout">The layout of the property map table.</param>
        /// <returns>The row.</returns>
        public static PropertyMapRow FromReader(IBinaryStreamReader reader, TableLayout layout)
        {
            return new PropertyMapRow(
                reader.ReadIndex((IndexSize) layout.Columns[0].Size),
                reader.ReadIndex((IndexSize) layout.Columns[1].Size));
        }

        /// <summary>
        /// Creates a new row for the property map metadata table.
        /// </summary>
        /// <param name="parent">The index into the TypeDef table that this mapping is associating to an property list.</param>
        /// <param name="propertyList">The index into the Event table indicating the first property that is defined in
        /// the property list.</param>
        public PropertyMapRow(uint parent, uint propertyList)
        {
            Parent = parent;
            PropertyList = propertyList;
        }

        /// <inheritdoc />
        public TableIndex TableIndex => TableIndex.PropertyMap;

        /// <summary>
        /// Gets an index into the TypeDef table that this mapping is associating to an property list.
        /// </summary>
        public uint Parent
        {
            get;
        }

        /// <summary>
        /// Gets an index into the Event table indicating the first property that is defined in the property list.
        /// </summary>
        public uint PropertyList
        {
            get;
        }

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer, TableLayout layout)
        {
            writer.WriteIndex(Parent, (IndexSize) layout.Columns[0].Size);
            writer.WriteIndex(PropertyList, (IndexSize) layout.Columns[1].Size);
        }

        /// <summary>
        /// Determines whether this row is considered equal to the provided property map row.
        /// </summary>
        /// <param name="other">The other row.</param>
        /// <returns><c>true</c> if the rows are equal, <c>false</c> otherwise.</returns>
        public bool Equals(PropertyMapRow other)
        {
            return Parent == other.Parent && PropertyList == other.PropertyList;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is PropertyMapRow other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) Parent * 397) ^ (int) PropertyList;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"({Parent:X8}, {PropertyList:X8})";
        }
        
    }
}