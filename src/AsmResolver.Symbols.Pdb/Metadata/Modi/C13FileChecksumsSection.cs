using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Metadata.Modi;

/// <summary>
/// Represents the C13 debug info section that contains file checksums.
/// </summary>
public class C13FileChecksumsSection : C13SubSection
{
    private IList<C13FileChecksum>? _checksums;

    /// <summary>
    /// Creates an empty file checksums section.
    /// </summary>
    public C13FileChecksumsSection()
        : base(C13SubSectionType.FileChecksums)
    {
    }

    /// <summary>
    /// Gets the checksums stored in the section.
    /// </summary>
    public IList<C13FileChecksum> Checksums
    {
        get
        {
            if (_checksums is null)
                Interlocked.CompareExchange(ref _checksums, GetChecksums(), null);
            return _checksums;
        }
    }

    /// <summary>
    /// Obtains the checksums stored in the section.
    /// </summary>
    /// <returns>The checksums.</returns>
    /// <remarks>
    /// This method is called upon initialization of the <see cref="Checksums"/> property.
    /// </remarks>
    protected virtual IList<C13FileChecksum> GetChecksums() => new List<C13FileChecksum>();

    /// <inheritdoc />
    protected override uint GetContentsLength() => (uint) Checksums.Sum(x => x.GetPhysicalSize());

    /// <inheritdoc />
    protected override void WriteContents(BinaryStreamWriter writer)
    {
        for (int i = 0; i < Checksums.Count; i++)
            Checksums[i].Write(writer);
    }
}
