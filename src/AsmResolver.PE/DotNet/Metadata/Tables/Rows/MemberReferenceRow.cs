using System;
using System.Collections;
using System.Collections.Generic;

namespace AsmResolver.PE.DotNet.Metadata.Tables.Rows
{
    /// <summary>
    /// Represents a single row in the member reference metadata table.
    /// </summary>
    public readonly struct MemberReferenceRow : IMetadataRow
    {
        /// <summary>
        /// Reads a single member reference row from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="layout">The layout of the member reference table.</param>
        /// <returns>The row.</returns>
        public static MemberReferenceRow FromReader(IBinaryStreamReader reader, TableLayout layout)
        {
            return new MemberReferenceRow(
                reader.ReadIndex((IndexSize) layout.Columns[0].Size),
                reader.ReadIndex((IndexSize) layout.Columns[1].Size),
                reader.ReadIndex((IndexSize) layout.Columns[2].Size));
        }
        
        /// <summary>
        /// Creates a new row for the member reference metadata table.
        /// </summary>
        /// <param name="parent">The MemberRefParent index indicating the parent member reference or definition that
        /// defines or can resolve the member reference.</param>
        /// <param name="name">The index into the #Strings heap containing the name of the member reference.</param>
        /// <param name="signature">The index into the #Blob heap containing the signature of the member.</param>
        public MemberReferenceRow(uint parent, uint name, uint signature)
        {
            Parent = parent;
            Name = name;
            Signature = signature;
        }

        /// <inheritdoc />
        public TableIndex TableIndex => TableIndex.MemberRef;

        /// <inheritdoc />
        public int Count => 3;

        /// <inheritdoc />
        public uint this[int index] => index switch
        {
            0 => Parent,
            1 => Name,
            2 => Signature,
            _ => throw new IndexOutOfRangeException()
        };

        /// <summary>
        /// Gets a MemberRefParent index (an index into either the TypeDef, TypeRef, ModuleRef, Method or TypeSpec table)
        /// indicating the parent member reference or definition that defines or can resolve the member reference.
        /// </summary>
        public uint Parent
        {
            get;
        }

        /// <summary>
        /// Gets an index into the #Strings heap containing the name of the member reference.
        /// </summary>
        /// <remarks>
        /// This value should always index a non-empty string.
        /// </remarks>
        public uint Name
        {
            get;
        }

        /// <summary>
        /// Gets an index into the #Blob heap containing the signature of the member. 
        /// </summary>
        /// <remarks>
        /// This value should always index a valid member signature. This value can also be used to determine whether
        /// the member reference is a field or a method.
        /// </remarks>
        public uint Signature
        {
            get;
        }

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer, TableLayout layout)
        {
            writer.WriteIndex(Parent, (IndexSize) layout.Columns[0].Size);
            writer.WriteIndex(Name, (IndexSize) layout.Columns[1].Size);
            writer.WriteIndex(Signature, (IndexSize) layout.Columns[2].Size);
        }

        /// <summary>
        /// Determines whether this row is considered equal to the provided member reference row.
        /// </summary>
        /// <param name="other">The other row.</param>
        /// <returns><c>true</c> if the rows are equal, <c>false</c> otherwise.</returns>
        public bool Equals(MemberReferenceRow other)
        {
            return Parent == other.Parent && Name == other.Name && Signature == other.Signature;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is MemberReferenceRow other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (int) Parent;
                hashCode = (hashCode * 397) ^ (int) Name;
                hashCode = (hashCode * 397) ^ (int) Signature;
                return hashCode;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"({Parent:X8}, {Name:X8}, {Signature:X8})";
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