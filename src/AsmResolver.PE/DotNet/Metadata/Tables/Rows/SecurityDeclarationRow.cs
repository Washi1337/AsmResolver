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
    public readonly struct SecurityDeclarationRow : IMetadataRow
    {
        public static SecurityDeclarationRow FromReader(IBinaryStreamReader reader, TableLayout layout)
        {
            return new SecurityDeclarationRow(
                (SecurityAction) reader.ReadUInt16(),
                reader.ReadIndex((IndexSize) layout.Columns[1].Size),
                reader.ReadIndex((IndexSize) layout.Columns[2].Size));
        }

        public SecurityDeclarationRow(SecurityAction action, uint parent, uint permissionSet)
        {
            Action = action;
            Parent = parent;
            PermissionSet = permissionSet;
        }
        
        public TableIndex TableIndex => TableIndex.DeclSecurity;

        /// <summary>
        /// Gets the action to be performed.
        /// </summary>
        public SecurityAction Action
        {
            get;
        }

        /// <summary>
        /// Gets a HasDeclSecurity index (an index into either the TypeDef, Method or Assembly table) that this
        /// security attribute is assigned to.
        /// </summary>
        public uint Parent
        {
            get;
        }

        /// <summary>
        /// Gets an index into the #Blob stream referencing the permission set assigned to the member.
        /// </summary>
        public uint PermissionSet
        {
            get;
        }

        /// <summary>
        /// Determines whether this row is considered equal to the provided security declaration row.
        /// </summary>
        /// <param name="other">The other row.</param>
        /// <returns><c>true</c> if the rows are equal, <c>false</c> otherwise.</returns>
        public bool Equals(SecurityDeclarationRow other)
        {
            return Action == other.Action && Parent == other.Parent && PermissionSet == other.PermissionSet;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is SecurityDeclarationRow other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (int) Action;
                hashCode = (hashCode * 397) ^ (int) Parent;
                hashCode = (hashCode * 397) ^ (int) PermissionSet;
                return hashCode;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"({(int) Action:X4}, {Parent:X8}, {PermissionSet:X8})";
        }
        
    }
}