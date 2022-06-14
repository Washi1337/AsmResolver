using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.IO;

namespace AsmResolver.Symbols.WindowsPdb.Msf;

/// <summary>
/// Represents a single stream in an Multi-Stream Format (MSF) file.
/// </summary>
public class MsfStream
{
    /// <summary>
    /// Creates a new MSF stream with the provided contents.
    /// </summary>
    /// <param name="data">The raw data of the stream.</param>
    public MsfStream(byte[] data)
        : this(new ByteArrayDataSource(data))
    {
    }

    /// <summary>
    /// Creates a new MSF stream with the provided data source as contents.
    /// </summary>
    /// <param name="contents">The data source containing the raw data of the stream.</param>
    public MsfStream(IDataSource contents)
    {
        Contents = contents;
        Blocks = Array.Empty<int>();
    }

    /// <summary>
    /// Initializes an MSF stream with a data source and a list of original block indices that the stream was based on.
    /// </summary>
    /// <param name="contents">The data source containing the raw data of the stream.</param>
    /// <param name="blocks">The original block indices.</param>
    public MsfStream(IDataSource contents, IEnumerable<int> blocks)
    {
        Contents = contents;
        Blocks = blocks.ToArray();
    }

    /// <summary>
    /// Gets or sets the contents of the stream.
    /// </summary>
    public IDataSource Contents
    {
        get;
        set;
    }

    /// <summary>
    /// Gets a collection of block indices that this stream was based of (if available).
    /// </summary>
    public IReadOnlyList<int> Blocks
    {
        get;
    }

    /// <summary>
    /// Creates a new binary reader that reads the raw contents of the stream.
    /// </summary>
    /// <returns>The constructed binary reader.</returns>
    public BinaryStreamReader CreateReader() => new(Contents, Contents.BaseAddress, 0, (uint) Contents.Length);
}
