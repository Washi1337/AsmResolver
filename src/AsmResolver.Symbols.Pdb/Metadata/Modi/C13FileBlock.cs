using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Metadata.Modi;

/// <summary>
/// Represents a single block in a CodeView C13 line number section.
/// </summary>
public class C13FileBlock : IWritable
{
    /// <summary>
    /// The size in bytes of a file block header.
    /// </summary>
    public const uint HeaderSize =
            sizeof(uint) // FileId
            + sizeof(uint) // LineCount
            + sizeof(uint) // BytesCount
        ;

    private IList<C13Line>? _lines;
    private IList<C13Column>? _columns;

    /// <summary>
    /// Gets or sets the identifier of the file the block is associated with.
    /// </summary>
    public uint FileId
    {
        get;
        set;
    }

    /// <summary>
    /// Gets the offset line mappings stored in the block.
    /// </summary>
    public IList<C13Line> Lines
    {
        get
        {
            if (_lines is null)
                Interlocked.CompareExchange(ref _lines, GetLines(), null);
            return _lines;
        }
    }

    /// <summary>
    /// When available, gets the column ranges associated with the <see cref="Lines"/>.
    /// </summary>
    public IList<C13Column> Columns
    {
        get
        {
            if (_columns is null)
                Interlocked.CompareExchange(ref _columns, GetColumns(), null);
            return _columns;
        }
    }

    /// <summary>
    /// Obtains the lines stored in the block.
    /// </summary>
    /// <returns>The lines.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="Lines"/> property.
    /// </remarks>
    protected virtual IList<C13Line> GetLines() => new List<C13Line>();

    /// <summary>
    /// Obtains the columns stored in the block.
    /// </summary>
    /// <returns>The columns.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="Columns"/> property.
    /// </remarks>
    protected virtual IList<C13Column> GetColumns() => new List<C13Column>();

    /// <inheritdoc />
    public uint GetPhysicalSize()
    {
        return HeaderSize
            + (uint) Lines.Sum(static x => x.GetPhysicalSize())
            + (uint) Columns.Sum(static x => x.GetPhysicalSize());
    }

    /// <inheritdoc />
    public void Write(BinaryStreamWriter writer)
    {
        writer.WriteUInt32(FileId);
        writer.WriteUInt32((uint) Lines.Count);
        writer.WriteUInt32(GetPhysicalSize());

        for (int i = 0; i < Lines.Count; i++)
            Lines[i].Write(writer);

        for (int i = 0; i < Columns.Count; i++)
            Columns[i].Write(writer);
    }
}
