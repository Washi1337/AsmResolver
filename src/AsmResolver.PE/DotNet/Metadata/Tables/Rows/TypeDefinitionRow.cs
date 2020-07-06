using System;
using System.Collections;
using System.Collections.Generic;

namespace AsmResolver.PE.DotNet.Metadata.Tables.Rows
{
    /// <summary>
    /// Represents a single row in the type definition metadata table.
    /// </summary>
    public readonly struct TypeDefinitionRow : IMetadataRow
    {
        /// <summary>
        /// Reads a single type definition row from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="layout">The layout of the type definition table.</param>
        /// <returns>The row.</returns>
        public static TypeDefinitionRow FromReader(IBinaryStreamReader reader, TableLayout layout)
        {
            return new TypeDefinitionRow(
                (TypeAttributes) reader.ReadUInt32(),
                reader.ReadIndex((IndexSize) layout.Columns[1].Size),
                reader.ReadIndex((IndexSize) layout.Columns[2].Size),
                reader.ReadIndex((IndexSize) layout.Columns[3].Size),
                reader.ReadIndex((IndexSize) layout.Columns[4].Size),
                reader.ReadIndex((IndexSize) layout.Columns[5].Size));
        }
        
        /// <summary>
        /// Creates a new row for the type definition metadata table.
        /// </summary>
        /// <param name="attributes">The attributes associated to the type.</param>
        /// <param name="name">The index into the #Strings heap containing the name of the type reference.</param>
        /// <param name="ns">The index into the #Strings heap containing the namespace of the type reference.</param>
        /// <param name="extends">The TypeDefOrRef coded index representing the base type of this type.</param>
        /// <param name="fieldList">The index into the Field (or FieldPtr) table, representing the first field defined
        /// in the type. </param>
        /// <param name="methodList">The index into the Method (or MethodPtr) table, representing the first method defined
        /// in the type. </param>
        public TypeDefinitionRow(TypeAttributes attributes, uint name, uint ns, uint extends, uint fieldList, uint methodList)
        {
            Attributes = attributes;
            Name = name;
            Namespace = ns;
            Extends = extends;
            FieldList = fieldList;
            MethodList = methodList;
        }

        /// <inheritdoc />
        public TableIndex TableIndex => TableIndex.TypeDef;

        /// <inheritdoc />
        public int Count => 6;

        /// <inheritdoc />
        public uint this[int index] => index switch
        {
            0 => (uint) Attributes,
            1 => Name,
            2 => Namespace,
            3 => Extends,
            4 => FieldList,
            5 => MethodList,
            _ => throw new IndexOutOfRangeException()
        };

        /// <summary>
        /// Gets the attributes associated to the type.
        /// </summary>
        public  TypeAttributes Attributes
        {
            get;
        }

        /// <summary>
        /// Gets an index into the #Strings heap containing the name of the type reference.
        /// </summary>
        /// <remarks>
        /// This value should always index a non-empty string.
        /// </remarks>
        public uint Name
        {
            get;
        }

        /// <summary>
        /// Gets an index into the #Strings heap containing the namespace of the type reference.
        /// </summary>
        /// <remarks>
        /// This value can be zero. If it is not, it should always index a non-empty string.
        /// </remarks>
        public uint Namespace
        {
            get;
        }

        /// <summary>
        /// Gets a TypeDefOrRef coded index (an index to a row in either the TypeRef, TypeDef or TypeSpec table)
        /// representing the base type of this type. 
        /// </summary>
        public uint Extends
        {
            get;
        }

        /// <summary>
        /// Gets an index into the Field (or FieldPtr) table, representing the first field defined in the type. 
        /// </summary>
        public uint FieldList
        {
            get;
        }

        /// <summary>
        /// Gets an index into the Method (or MethodPtr) table, representing the first method defined in the type. 
        /// </summary>
        public uint MethodList
        {
            get;
        }

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer, TableLayout layout)
        {
            writer.WriteUInt32((uint) Attributes);
            writer.WriteIndex(Name, (IndexSize) layout.Columns[1].Size);
            writer.WriteIndex(Namespace, (IndexSize) layout.Columns[2].Size);
            writer.WriteIndex(Extends, (IndexSize) layout.Columns[3].Size);
            writer.WriteIndex(FieldList, (IndexSize) layout.Columns[4].Size);
            writer.WriteIndex(MethodList, (IndexSize) layout.Columns[5].Size);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"({(uint) Attributes:X8}, {Name:X8}, {Namespace:X8}, {Extends:X8}, {FieldList:X8}, {MethodList:X8})";
        }

        /// <summary>
        /// Determines whether this row is considered equal to the provided type definition row.
        /// </summary>
        /// <param name="other">The other row.</param>
        /// <returns><c>true</c> if the rows are equal, <c>false</c> otherwise.</returns>
        public bool Equals(TypeDefinitionRow other)
        {
            return Attributes == other.Attributes
                   && Name == other.Name
                   && Namespace == other.Namespace 
                   && Extends == other.Extends
                   && FieldList == other.FieldList
                   && MethodList == other.MethodList;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is TypeDefinitionRow other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (int) Attributes;
                hashCode = (hashCode * 397) ^ (int) Name;
                hashCode = (hashCode * 397) ^ (int) Namespace;
                hashCode = (hashCode * 397) ^ (int) Extends;
                hashCode = (hashCode * 397) ^ (int) FieldList;
                hashCode = (hashCode * 397) ^ (int) MethodList;
                return hashCode;
            }
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
