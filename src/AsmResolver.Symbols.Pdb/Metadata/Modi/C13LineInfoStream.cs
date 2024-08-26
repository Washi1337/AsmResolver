using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Metadata.Modi;

/// <summary>
/// Represents the sub-stream containing C13 line information for a module.
/// </summary>
public class C13LineInfoStream : SegmentBase
{
    private IList<C13SubSection>? _sections;

    /// <summary>
    /// Gets the sub sections stored in the stream.
    /// </summary>
    public IList<C13SubSection> Sections
    {
        get
        {
            if (_sections is null)
                Interlocked.CompareExchange(ref _sections, GetSections(), null);
            return _sections;
        }
    }

    /// <summary>
    /// Obtains the sections stored in the stream.
    /// </summary>
    /// <returns>The sections.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="Sections"/> property.
    /// </remarks>
    protected virtual IList<C13SubSection> GetSections() => new List<C13SubSection>();

    /// <inheritdoc />
    public override uint GetPhysicalSize() => (uint) Sections.Sum(static x => x.GetPhysicalSize());

    /// <inheritdoc />
    public override void Write(BinaryStreamWriter writer)
    {
        for (int i = 0; i < Sections.Count; i++)
            Sections[i].Write(writer);
    }
}
