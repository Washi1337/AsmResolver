using System;
using System.Collections;
using System.Collections.Generic;

namespace AsmResolver.PE.DotNet.Metadata.Tables.Rows
{
    /// <summary>
    /// Represents a single row in the security declaration metadata table.
    /// </summary>
    public readonly struct SecurityDeclarationRow : IMetadataRow
    {
        /// <summary>
        /// Reads a single security declaration row from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="layout">The layout of the security declaration table.</param>
        /// <returns>The row.</returns>
        public static SecurityDeclarationRow FromReader(IBinaryStreamReader reader, TableLayout layout)
        {
            return new SecurityDeclarationRow(
                (SecurityAction) reader.ReadUInt16(),
                reader.ReadIndex((IndexSize) layout.Columns[1].Size),
                reader.ReadIndex((IndexSize) layout.Columns[2].Size));
        }

        /// <summary>
        /// Creates a new row for the security declaration metadata table.
        /// </summary>
        /// <param name="action">The action to be performed.</param>
        /// <param name="parent">The HasDeclSecurity index that this security attribute is assigned to.</param>
        /// <param name="permissionSet">The index into the #Blob stream referencing the permission set assigned to the member.</param>
        public SecurityDeclarationRow(SecurityAction action, uint parent, uint permissionSet)
        {
            Action = action;
            Parent = parent;
            PermissionSet = permissionSet;
        }

        /// <inheritdoc />
        public TableIndex TableIndex => TableIndex.DeclSecurity;

        /// <inheritdoc />
        public int Count => 3;

        /// <inheritdoc />
        public uint this[int index] => index switch
        {
            0 => (uint) Action,
            1 => Parent,
            2 => PermissionSet,
            _ => throw new IndexOutOfRangeException()
        };

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

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer, TableLayout layout)
        {
            writer.WriteUInt16((ushort) Action);
            writer.WriteIndex(Parent, (IndexSize) layout.Columns[1].Size);
            writer.WriteIndex(PermissionSet, (IndexSize) layout.Columns[2].Size);
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