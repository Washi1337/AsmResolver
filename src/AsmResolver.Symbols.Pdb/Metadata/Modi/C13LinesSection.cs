using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Metadata.Modi;

/// <summary>
/// Represents a section of CodeView C13 debug data containing offset to line number mappings.
/// </summary>
public class C13LinesSection : C13SubSection
{
    private IList<C13FileBlock>? _blocks;

    /// <summary>
    /// Creates an empty lines section.
    /// </summary>
    public C13LinesSection()
        : base(C13SubSectionType.Lines)
    {
    }

    /// <summary>
    /// Gets or sets the starting offset within the section contribution this line mapping is associated with.
    /// </summary>
    public uint SectionContributionOffset
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the index of the section this line mapping is associated with.
    /// </summary>
    public ushort SectionContributionIndex
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets attributes describing the contents of the section.
    /// </summary>
    public C13LinesAttributes Attributes
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the total size of the code.
    /// </summary>
    public uint CodeSize
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the section contains column ranges.
    /// </summary>
    public bool HasColumns
    {
        get => (Attributes & C13LinesAttributes.HasColumns) != 0;
        set => Attributes = (Attributes & ~C13LinesAttributes.HasColumns)
            | (value ? C13LinesAttributes.HasColumns : 0);
    }

    /// <summary>
    /// Gets a collection of file blocks stored in the section.
    /// </summary>
    public IList<C13FileBlock> Blocks
    {
        get
        {
            if (_blocks is null)
                Interlocked.CompareExchange(ref _blocks, GetBlocks(), null);
            return _blocks;
        }
    }

    /// <summary>
    /// Obtains the blocks stored in the section.
    /// </summary>
    /// <returns>The blocks.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="Blocks"/> property.
    /// </remarks>
    protected virtual IList<C13FileBlock> GetBlocks() => new List<C13FileBlock>();

    /// <inheritdoc />
    protected override uint GetContentsLength()
    {
        return sizeof(uint) // Offset
            + sizeof(ushort) // Index
            + sizeof(C13LinesAttributes) // Attributes
            + sizeof(uint) // Code size
            + (uint) Blocks.Sum(x => x.GetPhysicalSize()); // Blocks
    }

    /// <inheritdoc />
    protected override void WriteContents(BinaryStreamWriter writer)
    {
        writer.WriteUInt32(SectionContributionOffset);
        writer.WriteUInt16(SectionContributionIndex);
        writer.WriteUInt16((ushort) Attributes);
        writer.WriteUInt32(CodeSize);

        for (int i = 0; i < Blocks.Count; i++)
            Blocks[i].Write(writer);
    }
}
