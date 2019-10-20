namespace AsmResolver.PE.DotNet.Metadata.Tables.Rows
{
    /// <summary>
    /// Represents a single row in the generic parameter metadata table.
    /// </summary>
    public readonly struct GenericParameterRow : IMetadataRow
    {
        /// <summary>
        /// Reads a single generic parameter row from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="layout">The layout of the generic parameter table.</param>
        /// <returns>The row.</returns>
        public static GenericParameterRow FromReader(IBinaryStreamReader reader, TableLayout layout)
        {
            return new GenericParameterRow(
                reader.ReadUInt16(),
                (GenericParameterAttributes) reader.ReadUInt16(),
                reader.ReadIndex((IndexSize) layout.Columns[2].Size),
                reader.ReadIndex((IndexSize) layout.Columns[3].Size));
        }
        
        public GenericParameterRow(ushort number, GenericParameterAttributes attributes, uint owner, uint name)
        {
            Number = number;
            Attributes = attributes;
            Owner = owner;
            Name = name;
        }
        
        /// <inheritdoc />
        public TableIndex TableIndex => TableIndex.GenericParam;

        /// <summary>
        /// Gets the index of the generic parameter.
        /// </summary>
        public ushort Number
        {
            get;
        }

        /// <summary>
        /// Gets the attributes associated to the generic parameter.
        /// </summary>
        public GenericParameterAttributes Attributes
        {
            get;
        }

        /// <summary>
        /// Gets a TypeOrMethodDef index (an index into either the TypeDef or MethodDef table) indicating the owner
        /// of the generic parameter.
        /// </summary>
        public uint Owner
        {
            get;
        }

        /// <summary>
        /// Gets an index into the #Strings stream referencing the name of the generic parameter.
        /// </summary>
        public uint Name
        {
            get;
        }

        /// <summary>
        /// Determines whether this row is considered equal to the provided generic parameter row.
        /// </summary>
        /// <param name="other">The other row.</param>
        /// <returns><c>true</c> if the rows are equal, <c>false</c> otherwise.</returns>
        public bool Equals(GenericParameterRow other)
        {
            return Number == other.Number 
                   && Attributes == other.Attributes
                   && Owner == other.Owner 
                   && Name == other.Name;
        }

        public override bool Equals(object obj)
        {
            return obj is GenericParameterRow other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Number.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) Attributes;
                hashCode = (hashCode * 397) ^ (int) Owner;
                hashCode = (hashCode * 397) ^ (int) Name;
                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"({Number:X4}, {(int) Attributes:X4}, {Owner:X8}, {Name:X8})";
        }
        
    }
}