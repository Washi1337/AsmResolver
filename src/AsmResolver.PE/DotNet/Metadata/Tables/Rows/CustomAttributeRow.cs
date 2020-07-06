using System;
using System.Collections;
using System.Collections.Generic;

namespace AsmResolver.PE.DotNet.Metadata.Tables.Rows
{
    /// <summary>
    /// Represents a single row in the custom attribute metadata table.
    /// </summary>
    public readonly struct CustomAttributeRow : IMetadataRow
    {
        /// <summary>
        /// Reads a single custom attribute row from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="layout">The layout of the custom attribute table.</param>
        /// <returns>The row.</returns>
        public static CustomAttributeRow FromReader(IBinaryStreamReader reader, TableLayout layout)
        {
            return new CustomAttributeRow(
                reader.ReadIndex((IndexSize) layout.Columns[0].Size),
                reader.ReadIndex((IndexSize) layout.Columns[1].Size),
                reader.ReadIndex((IndexSize) layout.Columns[2].Size));
        }

        /// <summary>
        /// Creates a new row for the custom attribute metadata table.
        /// </summary>
        /// <param name="parent">The HasCustomAttribute index that this attribute is assigned to.</param>
        /// <param name="type">The CustomAttributeType index (an index into either the Method or MemberRef table) defining the
        /// constructor to call when initializing the custom attribute.</param>
        /// <param name="value">The index into the #Blob stream containing the arguments of the constructor call.</param>
        public CustomAttributeRow(uint parent, uint type, uint value)
        {
            Parent = parent;
            Type = type;
            Value = value;
        }

        /// <inheritdoc />
        public TableIndex TableIndex => TableIndex.CustomAttribute;

        /// <inheritdoc />
        public int Count => 3;
        
        /// <inheritdoc />
        public uint this[int index] => index switch
        {
            0 => Parent,
            1 => Type,
            2 => Value,
            _ => throw new IndexOutOfRangeException()
        };
        
        /// <summary>
        /// Gets a HasCustomAttribute index (an index into either the Method, Field, TypeRef, TypeDef,
        /// Param, InterfaceImpl, MemberRef, Module, DeclSecurity, Property, Event, StandAloneSig, ModuleRef,
        /// TypeSpec, Assembly, AssemblyRef, File, ExportedType, ManifestResource, GenericParam, GenericParamConstraint,
        /// or MethodSpec table) that this attribute is assigned to.
        /// </summary>
        public uint Parent
        {
            get;
        }

        /// <summary>
        /// Gets a CustomAttributeType index (an index into either the Method or MemberRef table) defining the
        /// constructor to call when initializing the custom attribute.
        /// </summary>
        public uint Type
        {
            get;
        }

        /// <summary>
        /// Gets an index into the #Blob stream containing the arguments of the constructor call.
        /// </summary>
        public uint Value
        {
            get;
        }

        /// <summary>
        /// Determines whether this row is considered equal to the provided custom attribute row.
        /// </summary>
        /// <param name="other">The other row.</param>
        /// <returns><c>true</c> if the rows are equal, <c>false</c> otherwise.</returns>
        public bool Equals(CustomAttributeRow other)
        {
            return Parent == other.Parent
                   && Type == other.Type
                   && Value == other.Value;
        }

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer, TableLayout layout)
        {
            writer.WriteIndex(Parent, (IndexSize) layout.Columns[0].Size);
            writer.WriteIndex(Type, (IndexSize) layout.Columns[1].Size);
            writer.WriteIndex(Value, (IndexSize) layout.Columns[2].Size);
        }

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is CustomAttributeRow other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (int) Parent;
                hashCode = (hashCode * 397) ^ (int) Type;
                hashCode = (hashCode * 397) ^ (int) Value;
                return hashCode;
            }
        }

        /// <inheritdoc />
        public override string ToString() => $"({Parent:X8}, {Type:X8}, {Value:X8})";

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