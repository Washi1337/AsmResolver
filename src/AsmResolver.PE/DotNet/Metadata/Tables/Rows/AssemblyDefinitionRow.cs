using System;
using System.Collections;
using System.Collections.Generic;

namespace AsmResolver.PE.DotNet.Metadata.Tables.Rows
{
    /// <summary>
    /// Represents a single row in the assembly definition metadata table.
    /// </summary>
    public readonly struct AssemblyDefinitionRow : IMetadataRow
    {
        /// <summary>
        /// Reads a single assembly definition row from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="layout">The layout of the assembly definition table.</param>
        /// <returns>The row.</returns>
        public static AssemblyDefinitionRow FromReader(IBinaryStreamReader reader, TableLayout layout)
        {
            return new AssemblyDefinitionRow(
                (AssemblyHashAlgorithm) reader.ReadUInt32(),
                reader.ReadUInt16(),
                reader.ReadUInt16(),
                reader.ReadUInt16(),
                reader.ReadUInt16(),
                (AssemblyAttributes) reader.ReadUInt32(),
                reader.ReadIndex((IndexSize) layout.Columns[6].Size),
                reader.ReadIndex((IndexSize) layout.Columns[7].Size),
                reader.ReadIndex((IndexSize) layout.Columns[8].Size));
        }

        /// <summary>
        /// Creates a new row for the assembly definition table.
        /// </summary>
        /// <param name="hashAlgorithm">The hashing algorithm used to sign the assembly.</param>
        /// <param name="majorVersion">The major version number of the assembly.</param>
        /// <param name="minorVersion">The minor version number of the assembly.</param>
        /// <param name="buildNumber">The build version number of the assembly.</param>
        /// <param name="revisionNumber">The revision version number of the assembly.</param>
        /// <param name="attributes">The attributes associated to the assembly.</param>
        /// <param name="publicKey">The index into the #Blob stream referencing the public key of the assembly to use
        /// for verification of a signature, or 0 if the assembly was not signed.</param>
        /// <param name="name">The index into the #Strings stream referencing the name of the assembly.</param>
        /// <param name="culture">The index into the #Strings stream referencing the locale string of the assembly.</param>
        public AssemblyDefinitionRow(AssemblyHashAlgorithm hashAlgorithm, 
            ushort majorVersion, ushort minorVersion, ushort buildNumber, ushort revisionNumber,
            AssemblyAttributes attributes, uint publicKey, uint name, uint culture)
        {
            HashAlgorithm = hashAlgorithm;
            MajorVersion = majorVersion;
            MinorVersion = minorVersion;
            BuildNumber = buildNumber;
            RevisionNumber = revisionNumber;
            Attributes = attributes;
            PublicKey = publicKey;
            Name = name;
            Culture = culture;
        }
        
        /// <inheritdoc />
        public TableIndex TableIndex => TableIndex.Assembly;

        /// <inheritdoc />
        public int Count => 9;
        
        /// <inheritdoc />
        public uint this[int index] => index switch
        {
            0 => (uint) HashAlgorithm,
            1 => MajorVersion,
            2 => MinorVersion,
            3 => BuildNumber,
            4 => RevisionNumber,
            5 => (uint) Attributes,
            6 => PublicKey,
            7 => Name,
            8 => Culture,
            _ => throw new IndexOutOfRangeException()
        };

        /// <summary>
        /// Gets the hashing algorithm that was used to sign the assembly.
        /// </summary>
        public AssemblyHashAlgorithm HashAlgorithm
        {
            get;
        }

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
        /// Gets an index into the #Blob stream referencing the public key of the assembly to use for verification of
        /// a signature.
        /// </summary>
        /// <remarks>
        /// When this field is set to zero, no public key is stored.
        /// </remarks>
        public uint PublicKey
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

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer, TableLayout layout)
        {
            writer.WriteUInt32((uint) HashAlgorithm);
            writer.WriteUInt16(MajorVersion);
            writer.WriteUInt16(MinorVersion);
            writer.WriteUInt16(BuildNumber);
            writer.WriteUInt16(RevisionNumber);
            writer.WriteUInt32((uint) Attributes);
            writer.WriteIndex(PublicKey, (IndexSize) layout.Columns[6].Size);
            writer.WriteIndex(Name, (IndexSize) layout.Columns[7].Size);
            writer.WriteIndex(Culture, (IndexSize) layout.Columns[8].Size);
        }

        /// <summary>
        /// Determines whether this row is considered equal to the provided assembly definition row.
        /// </summary>
        /// <param name="other">The other row.</param>
        /// <returns><c>true</c> if the rows are equal, <c>false</c> otherwise.</returns>
        public bool Equals(AssemblyDefinitionRow other)
        {
            return HashAlgorithm == other.HashAlgorithm 
                   && MajorVersion == other.MajorVersion
                   && MinorVersion == other.MinorVersion 
                   && BuildNumber == other.BuildNumber
                   && RevisionNumber == other.RevisionNumber
                   && Attributes == other.Attributes
                   && PublicKey == other.PublicKey
                   && Name == other.Name
                   && Culture == other.Culture;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is AssemblyDefinitionRow other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (int) HashAlgorithm;
                hashCode = (hashCode * 397) ^ MajorVersion.GetHashCode();
                hashCode = (hashCode * 397) ^ MinorVersion.GetHashCode();
                hashCode = (hashCode * 397) ^ BuildNumber.GetHashCode();
                hashCode = (hashCode * 397) ^ RevisionNumber.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) Attributes;
                hashCode = (hashCode * 397) ^ (int) PublicKey;
                hashCode = (hashCode * 397) ^ (int) Name;
                hashCode = (hashCode * 397) ^ (int) Culture;
                return hashCode;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"({(int) HashAlgorithm:X8}, {MajorVersion:X4}, {MinorVersion:X4}, {BuildNumber:X4}, {RevisionNumber:X4}, {(int) Attributes:X8}, {PublicKey:X8}, {Name:X8}, {Culture:X8})";
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