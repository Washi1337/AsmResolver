using System;
using System.Collections;
using System.Collections.Generic;

namespace AsmResolver.PE.DotNet.Metadata.Tables.Rows
{
    /// <summary>
    /// Represents a single row in the assembly definition metadata table.
    /// </summary>
    public readonly struct AssemblyReferenceRow : IMetadataRow
    {
        /// <summary>
        /// Reads a single assembly definition row from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="layout">The layout of the assembly definition table.</param>
        /// <returns>The row.</returns>
        public static AssemblyReferenceRow FromReader(IBinaryStreamReader reader, TableLayout layout)
        {
            return new AssemblyReferenceRow(
                reader.ReadUInt16(),
                reader.ReadUInt16(),
                reader.ReadUInt16(),
                reader.ReadUInt16(),
                (AssemblyAttributes) reader.ReadUInt32(),
                reader.ReadIndex((IndexSize) layout.Columns[5].Size),
                reader.ReadIndex((IndexSize) layout.Columns[6].Size),
                reader.ReadIndex((IndexSize) layout.Columns[7].Size),
                reader.ReadIndex((IndexSize) layout.Columns[8].Size));
        }

        /// <summary>
        /// Creates a new row for the assembly reference table.
        /// </summary>
        /// <param name="majorVersion">The major version number of the assembly.</param>
        /// <param name="minorVersion">The minor version number of the assembly.</param>
        /// <param name="buildNumber">The build version number of the assembly.</param>
        /// <param name="revisionNumber">The revision version number of the assembly.</param>
        /// <param name="attributes">The attributes associated to the assembly.</param>
        /// <param name="publicKeyOrToken">The index into the #Blob stream referencing the public key or token of the
        /// assembly to use for verification of a signature, or 0 if the assembly was not signed.</param>
        /// <param name="name">The index into the #Strings stream referencing the name of the assembly.</param>
        /// <param name="culture">The index into the #Strings stream referencing the locale string of the assembly.</param>
        /// <param name="hashValue">The index into the #Blob stream referencing the hash value of the assembly reference.</param>
        public AssemblyReferenceRow(ushort majorVersion, ushort minorVersion, ushort buildNumber, ushort revisionNumber, 
            AssemblyAttributes attributes, uint publicKeyOrToken, uint name, uint culture, uint hashValue)
        {
            MajorVersion = majorVersion;
            MinorVersion = minorVersion;
            BuildNumber = buildNumber;
            RevisionNumber = revisionNumber;
            Attributes = attributes;
            PublicKeyOrToken = publicKeyOrToken;
            Name = name;
            Culture = culture;
            HashValue = hashValue;
        }
        
        /// <inheritdoc />
        public TableIndex TableIndex => TableIndex.Assembly;

        /// <inheritdoc />
        public int Count => 9;

        /// <inheritdoc />
        public uint this[int index] => index switch
        {
            0 => MajorVersion,
            1 => MinorVersion,
            2 => BuildNumber,
            3 => RevisionNumber,
            4 => (uint) Attributes,
            5 => PublicKeyOrToken,
            6 => Name,
            7 => Culture,
            8 => HashValue,
            _ => throw new IndexOutOfRangeException()
        };
        
        /// <summary>
        /// Gets the major version number of the assembly.
        /// </summary>
        public ushort MajorVersion
        {
            get;
        }

        /// <summary>
        /// Gets the minor version number of the assembly.
        /// </summary>
        public ushort MinorVersion
        {
            get;
        }
        
        /// <summary>
        /// Gets the build number of the assembly.
        /// </summary>
        public ushort BuildNumber
        {
            get;
        }

        /// <summary>
        /// Gets the revision number of the assembly.
        /// </summary>
        public ushort RevisionNumber
        {
            get;
        }

        /// <summary>
        /// Gets the attributes associated to the assembly.
        /// </summary>
        public AssemblyAttributes Attributes
        {
            get;
        }

        /// <summary>
        /// Gets an index into the #Blob stream referencing the public key or token of the assembly to use for
        /// verification of a signature.
        /// </summary>
        /// <remarks>
        /// When this field is set to zero, no public key or token is stored.
        /// </remarks>
        public uint PublicKeyOrToken
        {
            get;
        }

        /// <summary>
        /// Gets an index into the #Strings stream referencing the name of the assembly.
        /// </summary>
        public uint Name
        {
            get;
        }

        /// <summary>
        /// Gets an index into the #Strings stream referencing the locale string of the assembly.
        /// </summary>
        /// <remarks>
        /// When this field is set to zero, the default culture is used.
        /// </remarks>
        public uint Culture
        {
            get;
        }

        /// <summary>
        /// Gets an index into the #Blob stream referencing the hash value of the assembly reference.
        /// </summary>
        public uint HashValue
        {
            get;
        }

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer, TableLayout layout)
        {
            writer.WriteUInt16(MajorVersion);
            writer.WriteUInt16(MinorVersion);
            writer.WriteUInt16(BuildNumber);
            writer.WriteUInt16(RevisionNumber);
            writer.WriteUInt32((uint) Attributes);
            writer.WriteIndex(PublicKeyOrToken, (IndexSize) layout.Columns[5].Size);
            writer.WriteIndex(Name, (IndexSize) layout.Columns[6].Size);
            writer.WriteIndex(Culture, (IndexSize) layout.Columns[7].Size);
            writer.WriteIndex(HashValue, (IndexSize) layout.Columns[8].Size);
        }

        /// <summary>
        /// Determines whether this row is considered equal to the provided assembly reference row.
        /// </summary>
        /// <param name="other">The other row.</param>
        /// <returns><c>true</c> if the rows are equal, <c>false</c> otherwise.</returns>
        public bool Equals(AssemblyReferenceRow other)
        {
            return MajorVersion == other.MajorVersion
                   && MinorVersion == other.MinorVersion
                   && BuildNumber == other.BuildNumber
                   && RevisionNumber == other.RevisionNumber 
                   && Attributes == other.Attributes
                   && PublicKeyOrToken == other.PublicKeyOrToken 
                   && Name == other.Name
                   && Culture == other.Culture
                   && HashValue == other.HashValue;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is AssemblyReferenceRow other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = MajorVersion.GetHashCode();
                hashCode = (hashCode * 397) ^ MinorVersion.GetHashCode();
                hashCode = (hashCode * 397) ^ BuildNumber.GetHashCode();
                hashCode = (hashCode * 397) ^ RevisionNumber.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) Attributes;
                hashCode = (hashCode * 397) ^ (int) PublicKeyOrToken;
                hashCode = (hashCode * 397) ^ (int) Name;
                hashCode = (hashCode * 397) ^ (int) Culture;
                hashCode = (hashCode * 397) ^ (int) HashValue;
                return hashCode;
            }
        }
        
        /// <inheritdoc />
        public override string ToString()
        {
            return $"({MajorVersion:X4}, {MinorVersion:X4}, {BuildNumber:X4}, {RevisionNumber:X4}, {(int) Attributes:X8}, {PublicKeyOrToken:X8}, {Name:X8}, {Culture:X8})";
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