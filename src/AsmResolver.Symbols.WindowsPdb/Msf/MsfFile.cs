using System;
using System.Collections.Generic;
using System.Threading;
using AsmResolver.IO;

namespace AsmResolver.Symbols.WindowsPdb.Msf;

/// <summary>
/// Models a file that is in the Microsoft Multi-Stream Format (MSF).
/// </summary>
public class MsfFile
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

    private uint _blockSize;
    private IList<MsfStream>? _streams;

    /// <summary>
    /// Gets or sets the size of each block in the MSF file.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Occurs when the provided value is neither 512, 1024, 2048 or 4096.
    /// </exception>
    public uint BlockSize
    {
        get => _blockSize;
        set
        {
            if (_blockSize is 512 or 1024 or 2048 or 4096)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    "Block size must be either 512, 1024, 2048 or 4096 bytes.");
            }

            _blockSize = value;
        }
    }

    /// <summary>
    /// Gets a collection of streams that are present in the MSF file.
    /// </summary>
    public IList<MsfStream> Streams
    {
        get
        {
            if (_streams is null)
                Interlocked.CompareExchange(ref _streams, GetStreams(), null);
            return _streams;
        }
    }

    /// <summary>
    /// Creates a new empty MSF file with a default block size of 4096.
    /// </summary>
    public MsfFile()
        : this(4096)
    {
    }

    /// <summary>
    /// Creates a new empty MSF file with the provided block size.
    /// </summary>
    /// <param name="blockSize">The block size to use. This must be a value of 512, 1024, 2048 or 4096.</param>
    /// <exception cref="ArgumentOutOfRangeException">Occurs when an invalid block size was provided.</exception>
    public MsfFile(uint blockSize)
    {
        BlockSize = blockSize;
    }

    /// <summary>
    /// Reads an MSF file from a file on the disk.
    /// </summary>
    /// <param name="path">The path to the file to read.</param>
    /// <returns>The read MSF file.</returns>
    public static MsfFile FromFile(string path) => FromFile(UncachedFileService.Instance.OpenFile(path));

    /// <summary>
    /// Reads an MSF file from an input file.
    /// </summary>
    /// <param name="file">The file to read.</param>
    /// <returns>The read MSF file.</returns>
    public static MsfFile FromFile(IInputFile file) => FromReader(file.CreateReader());

    /// <summary>
    /// Interprets a byte array as an MSF file.
    /// </summary>
    /// <param name="data">The data to interpret.</param>
    /// <returns>The read MSF file.</returns>
    public static MsfFile FromBytes(byte[] data) => FromReader(ByteArrayDataSource.CreateReader(data));

    /// <summary>
    /// Reads an MSF file from the provided input stream reader.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <returns>The read MSF file.</returns>
    public static MsfFile FromReader(BinaryStreamReader reader) => new SerializedMsfFile(reader);

    /// <summary>
    /// Obtains the list of streams stored in the MSF file.
    /// </summary>
    /// <returns>The streams.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="Streams"/> property.
    /// </remarks>
    protected virtual IList<MsfStream> GetStreams() => new List<MsfStream>();
}
