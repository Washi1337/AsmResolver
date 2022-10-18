using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using AsmResolver.Collections;
using AsmResolver.IO;
using AsmResolver.Symbols.Pdb.Msf.Builder;

namespace AsmResolver.Symbols.Pdb.Msf;

/// <summary>
/// Models a file that is in the Microsoft Multi-Stream Format (MSF).
/// </summary>
public class MsfFile
{
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
            if (value is not (512 or 1024 or 2048 or 4096))
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
    public static MsfFile FromBytes(byte[] data) => FromReader(new BinaryStreamReader(data));

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
    protected virtual IList<MsfStream> GetStreams() => new OwnedCollection<MsfFile, MsfStream>(this);

    /// <summary>
    /// Reconstructs and writes the MSF file to the disk.
    /// </summary>
    /// <param name="path">The path of the file to write to.</param>
    public void Write(string path)
    {
        using var fs = File.Create(path);
        Write(fs);
    }

    /// <summary>
    /// Reconstructs and writes the MSF file to an output stream.
    /// </summary>
    /// <param name="stream">The output stream.</param>
    public void Write(Stream stream) => Write(new BinaryStreamWriter(stream));

    /// <summary>
    /// Reconstructs and writes the MSF file to an output stream.
    /// </summary>
    /// <param name="writer">The output stream.</param>
    public void Write(IBinaryStreamWriter writer) => Write(writer, SequentialMsfFileBuilder.Instance);

    /// <summary>
    /// Reconstructs and writes the MSF file to an output stream.
    /// </summary>
    /// <param name="writer">The output stream.</param>
    /// <param name="builder">The builder to use for reconstructing the MSF file.</param>
    public void Write(IBinaryStreamWriter writer, IMsfFileBuilder builder) => builder.CreateFile(this).Write(writer);
}
