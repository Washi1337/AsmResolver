using System;
using System.Collections;
using System.Collections.Generic;

namespace AsmResolver.PE.DotNet.Metadata.Tables.Rows
{
    /// <summary>
    /// Represents a single row in the method definition metadata table.
    /// </summary>
    public readonly struct MethodDefinitionRow : IMetadataRow
    {
        /// <summary>
        /// Reads a single method definition row from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="layout">The layout of the method definition table.</param>
        /// <param name="referenceResolver">The resolver to use for resolving the virtual address of the method body.</param>
        /// <returns>The row.</returns>
        public static MethodDefinitionRow FromReader(IBinaryStreamReader reader, TableLayout layout, ISegmentReferenceResolver referenceResolver)
        {
            return new MethodDefinitionRow(
                referenceResolver.GetReferenceToRva(reader.ReadUInt32()),
                (MethodImplAttributes) reader.ReadUInt16(),
                (MethodAttributes) reader.ReadUInt16(),
                reader.ReadIndex((IndexSize) layout.Columns[3].Size),
                reader.ReadIndex((IndexSize) layout.Columns[4].Size),
                reader.ReadIndex((IndexSize) layout.Columns[5].Size));
        }

        /// <summary>
        /// Creates a new row for the method definition metadata table.
        /// </summary>
        /// <param name="body">The reference to the beginning of the method body. </param>
        /// <param name="implAttributes">The characteristics of the implementation of the method body.</param>
        /// <param name="attributes">The attributes associated to the method.</param>
        /// <param name="name">The index into the #Strings heap containing the name of the type reference.</param>
        /// <param name="signature">The index into the #Blob heap containing the signature of the method.</param>
        /// <param name="parameterList">The index into the Param (or ParamPtr) table, representing the first parameter
        /// that this method defines.</param>
        public MethodDefinitionRow(ISegmentReference body, MethodImplAttributes implAttributes, MethodAttributes attributes, 
            uint name, uint signature, uint parameterList)
        {
            Body = body;
            ImplAttributes = implAttributes;
            Attributes = attributes;
            Name = name;
            Signature = signature;
            ParameterList = parameterList;
        }

        /// <inheritdoc />
        public TableIndex TableIndex => TableIndex.Method;

        /// <inheritdoc />
        public int Count => 6;

        /// <inheritdoc />
        public uint this[int index] => index switch
        {
            0 => Body?.Rva ?? 0,
            1 => (uint) ImplAttributes,
            2 => (uint) Attributes,
            3 => Name,
            4 => Signature,
            5 => ParameterList,
            _ => throw new IndexOutOfRangeException()
        };

        /// <summary>
        /// Gets a reference to the beginning of the method body. 
        /// </summary>
        /// <remarks>
        /// This field deviates from the original specification as described in ECMA-335. It replaces the RVA column of
        /// the method definition row. Only the RVA of this reference is only considered when comparing two method definition
        /// rows for equality.
        ///
        /// If this value is null, the method does not define any method body.
        /// </remarks>
        public ISegmentReference Body
        {
            get;
        }

        /// <summary>
        /// Gets the characteristics of the implementation of the method body.
        /// </summary>
        /// <remarks>
        /// These attributes dictate the format of <see cref="Body"/>.
        /// </remarks>
        public MethodImplAttributes ImplAttributes
        {
            get;
        }

        /// <summary>
        /// Gets the attributes associated to the method.
        /// </summary>
        public MethodAttributes Attributes
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
        /// Gets an index into the #Blob heap containing the signature of the method. This includes the return type,
        /// as well as parameter types.
        /// </summary>
        /// <remarks>
        /// This value should always index a valid method signature.
        /// </remarks>
        public uint Signature
        {
            get;
        }

        /// <summary>
        /// Gets an index into the Param (or ParamPtr) table, representing the first parameter that this method defines. 
        /// </summary>
        public uint ParameterList
        {
            get;
        }

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer, TableLayout layout)
        {
            writer.WriteUInt32(Body?.Rva ?? 0);
            writer.WriteUInt16((ushort) ImplAttributes);
            writer.WriteUInt16((ushort) Attributes);
            writer.WriteIndex(Name, (IndexSize) layout.Columns[3].Size);
            writer.WriteIndex(Signature, (IndexSize) layout.Columns[4].Size);
            writer.WriteIndex(ParameterList, (IndexSize) layout.Columns[5].Size);
        }

        /// <summary>
        /// Determines whether this row is considered equal to the provided method definition row.
        /// </summary>
        /// <param name="other">The other row.</param>
        /// <returns><c>true</c> if the rows are equal, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// When comparing both method bodies, only the RVA is considered in this equality test. The exact type is ignored.
        /// </remarks>
        public bool Equals(MethodDefinitionRow other)
        {
            return Body?.Rva == other.Body?.Rva
                   && ImplAttributes == other.ImplAttributes
                   && Attributes == other.Attributes 
                   && Name == other.Name
                   && Signature == other.Signature
                   && ParameterList == other.ParameterList;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is MethodDefinitionRow other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (int) (Body?.Rva ?? 0);
                hashCode = (hashCode * 397) ^ (int) ImplAttributes;
                hashCode = (hashCode * 397) ^ (int) Attributes;
                hashCode = (hashCode * 397) ^ (int) Name;
                hashCode = (hashCode * 397) ^ (int) Signature;
                hashCode = (hashCode * 397) ^ (int) ParameterList;
                return hashCode;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"({Body?.Rva ?? 0:X8}, {(int)ImplAttributes:X4}, {(int) Attributes:X4}, {Name:X8}, {Signature:X8}, {ParameterList:X8})";
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