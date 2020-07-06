using System;
using System.Collections;
using System.Collections.Generic;

namespace AsmResolver.PE.DotNet.Metadata.Tables.Rows
{
    /// <summary>
    /// Represents a single row in the type reference metadata table.
    /// </summary>
    public struct TypeReferenceRow : IMetadataRow
    {
        /// <summary>
        /// Reads a single type reference row from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="layout">The layout of the type reference table.</param>
        /// <returns>The row.</returns>
        public static TypeReferenceRow FromReader(IBinaryStreamReader reader, TableLayout layout)
        {
            return new TypeReferenceRow(
                reader.ReadIndex((IndexSize) layout.Columns[0].Size),
                reader.ReadIndex((IndexSize) layout.Columns[1].Size),
                reader.ReadIndex((IndexSize) layout.Columns[2].Size));
        }

        /// <summary>
        /// Creates a new row for the type reference metadata table.
        /// </summary>
        /// <param name="resolutionScope">The  ResolutionScope coded index  containing the scope that can resolve this
        /// type reference. </param>
        /// <param name="name">The index into the #Strings heap containing the name of the type reference.</param>
        /// <param name="ns">The index into the #Strings heap containing the namespace of the type reference.</param>
        public TypeReferenceRow(uint resolutionScope, uint name, uint ns)
        {
            ResolutionScope = resolutionScope;
            Name = name;
            Namespace = ns;
        }

        /// <inheritdoc />
        public TableIndex TableIndex => TableIndex.TypeRef;

        /// <inheritdoc />
        public int Count => 3;

        /// <inheritdoc />
        public uint this[int index] => index switch
        {
            0 => ResolutionScope,
            1 => Name,
            2 => Namespace,
            _ => throw new IndexOutOfRangeException()
        };

        /// <summary>
        /// Gets a ResolutionScope coded index (an index to a row in either the Module, ModuleRef, AssemblyRef or TypeRef table)
        /// containing the scope that can resolve this type reference. 
        /// </summary>
        public uint ResolutionScope
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

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer, TableLayout layout)
        {
            writer.WriteIndex(ResolutionScope, (IndexSize) layout.Columns[0].Size);
            writer.WriteIndex(Name, (IndexSize) layout.Columns[1].Size);
            writer.WriteIndex(Namespace, (IndexSize) layout.Columns[2].Size);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"({ResolutionScope:X8}, {Name:X8}, {Namespace:X8})";
        }

        /// <summary>
        /// Determines whether this row is considered equal to the provided type reference row.
        /// </summary>
        /// <param name="other">The other row.</param>
        /// <returns><c>true</c> if the rows are equal, <c>false</c> otherwise.</returns>
        public bool Equals(TypeReferenceRow other)
        {
            return ResolutionScope == other.ResolutionScope 
                   && Name == other.Name 
                   && Namespace == other.Namespace;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is TypeReferenceRow other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (int) ResolutionScope;
                hashCode = (hashCode * 397) ^ (int) Name;
                hashCode = (hashCode * 397) ^ (int) Namespace;
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