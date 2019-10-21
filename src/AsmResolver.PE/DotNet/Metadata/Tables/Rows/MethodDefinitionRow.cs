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
    /// Represents a single row in the method definition metadata table.
    /// </summary>
    public readonly struct MethodDefinitionRow : IMetadataRow
    {
        /// <summary>
        /// Reads a single method definition row from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="layout">The layout of the method definition table.</param>
        /// <returns>The row.</returns>
        public static MethodDefinitionRow FromReader(IBinaryStreamReader reader, TableLayout layout)
        {
            return new MethodDefinitionRow(
                reader.ReadUInt32(),
                (MethodImplAttributes) reader.ReadUInt16(),
                (MethodAttributes) reader.ReadUInt16(),
                reader.ReadIndex((IndexSize) layout.Columns[3].Size),
                reader.ReadIndex((IndexSize) layout.Columns[4].Size),
                reader.ReadIndex((IndexSize) layout.Columns[5].Size));
        }

        public MethodDefinitionRow(uint rva, MethodImplAttributes implAttributes, MethodAttributes attributes, 
            uint name, uint signature, uint parameterList)
        {
            Rva = rva;
            ImplAttributes = implAttributes;
            Attributes = attributes;
            Name = name;
            Signature = signature;
            ParameterList = parameterList;
        }

        /// <inheritdoc />
        public TableIndex TableIndex => TableIndex.Method;

        /// <summary>
        /// Gets the starting virtual address of the method body. 
        /// </summary>
        /// <remarks>
        /// If this value is zero, the method does not define any method body.
        /// </remarks>
        public uint Rva
        {
            get;
        }

        /// <summary>
        /// Gets the characteristics of the implementation of the method body.
        /// </summary>
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
            writer.WriteUInt32(Rva);
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
        public bool Equals(MethodDefinitionRow other)
        {
            return Rva == other.Rva
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
                int hashCode = (int) Rva;
                hashCode = (hashCode * 397) ^ (int) ImplAttributes;
                hashCode = (hashCode * 397) ^ (int) Attributes;
                hashCode = (hashCode * 397) ^ (int) Name;
                hashCode = (hashCode * 397) ^ (int) Signature;
                hashCode = (hashCode * 397) ^ (int) ParameterList;
                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"({Rva:X8}, {(int)ImplAttributes:X4}, {(int) Attributes:X4}, {Name:X8}, {Signature:X8}, {ParameterList:X8})";
        }
        
    }
}