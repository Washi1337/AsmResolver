using System;
using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Msf;

/// <summary>
/// Represents the first block in a Multi-Stream Format (MSF) file.
/// </summary>
public sealed class MsfSuperBlock : SegmentBase
{
    // Used in MSF v2.0
    internal static readonly byte[] SmallMsfSignature =
    {
        0x4d, 0x69, 0x63, 0x72, 0x6f, 0x73, 0x6f, 0x66, 0x74, 0x20, 0x43, 0x2f, 0x43, 0x2b, 0x2b, 0x20, 0x70, 0x72,
        0x6f, 0x67, 0x72, 0x61, 0x6d, 0x20, 0x64, 0x61, 0x74, 0x61, 0x62, 0x61, 0x73, 0x65, 0x20, 0x32, 0x2e, 0x30,
        0x30, 0x0d, 0x0a, 0x1a, 0x4a, 0x47
    };

    // Used in MSF v7.0
    internal static readonly byte[] BigMsfSignature =
    {
        0x4d, 0x69, 0x63, 0x72, 0x6f, 0x73, 0x6f, 0x66, 0x74, 0x20, 0x43, 0x2f, 0x43, 0x2b, 0x2b, 0x20,
        0x4d, 0x53, 0x46, 0x20, 0x37, 0x2e, 0x30, 0x30, 0x0d, 0x0a, 0x1a, 0x44, 0x53, 0x00, 0x00, 0x00
    };

    /// <summary>
    /// Gets or sets the magic file signature in the super block, identifying the format version of the MSF file.
    /// </summary>
    public byte[] Signature
    {
        get;
        set;
    } = (byte[]) BigMsfSignature.Clone();

    /// <summary>
    /// Gets or sets the size of an individual block in bytes.
    /// </summary>
    public uint BlockSize
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the index of the block containing a bitfield indicating which blocks in the entire MSF file are
    /// in use or not.
    /// </summary>
    public uint FreeBlockMapIndex
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the total number of blocks in the MSF file.
    /// </summary>
    public uint BlockCount
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the number of bytes of the stream directory in the MSF file.
    /// </summary>
    public uint DirectoryByteCount
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the index of the block containing all block indices that make up the stream directory of the MSF
    /// file.
    /// </summary>
    public uint DirectoryMapIndex
    {
        get;
        set;
    }

    /// <summary>
    /// Reads a single MSF super block from the provided input stream.
    /// </summary>
    /// <param name="reader">The input stream.</param>
    /// <returns>The parsed MSF super block.</returns>
    /// <exception cref="BadImageFormatException">Occurs when the super block is malformed.</exception>
    public static MsfSuperBlock FromReader(ref BinaryStreamReader reader)
    {
        var result = new MsfSuperBlock();

        // Check MSF header.
        result.Signature = new byte[BigMsfSignature.Length];
        int count = reader.ReadBytes(result.Signature, 0, result.Signature.Length);
        if (count != BigMsfSignature.Length || !ByteArrayEqualityComparer.Instance.Equals(result.Signature, BigMsfSignature))
            throw new BadImageFormatException("File does not start with a valid or supported MSF file signature.");

        result.BlockSize = reader.ReadUInt32();
        if (result.BlockSize is not (512 or 1024 or 2048 or 4096))
            throw new BadImageFormatException("Block size must be either 512, 1024, 2048 or 4096 bytes.");

        // We don't really use the free block map as we are not fully implementing the NTFS-esque file system, but we
        // validate its contents regardless as a sanity check.
        result.FreeBlockMapIndex = reader.ReadUInt32();
        if (result.FreeBlockMapIndex is not (1 or 2))
            throw new BadImageFormatException($"Free block map index must be 1 or 2, but was {result.FreeBlockMapIndex}.");

        result.BlockCount = reader.ReadUInt32();

        result.DirectoryByteCount = reader.ReadUInt32();
        reader.Offset += sizeof(uint);
        result.DirectoryMapIndex = reader.ReadUInt32();

        return result;
    }

    /// <inheritdoc />
    public override uint GetPhysicalSize() => (uint) BigMsfSignature.Length + sizeof(uint) * 6;

    /// <inheritdoc />
    public override void Write(BinaryStreamWriter writer)
    {
        writer.WriteBytes(Signature);
        writer.WriteUInt32(BlockSize);
        writer.WriteUInt32(FreeBlockMapIndex);
        writer.WriteUInt32(BlockCount);
        writer.WriteUInt32(DirectoryByteCount);
        writer.WriteUInt32(0);
        writer.WriteUInt32(DirectoryMapIndex);
    }

}
