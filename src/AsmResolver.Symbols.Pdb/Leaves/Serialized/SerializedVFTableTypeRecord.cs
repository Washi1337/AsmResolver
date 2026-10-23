using System.Collections.Generic;
using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Leaves.Serialized;

/// <summary>
/// Provides a lazily initialized implementation of <see cref="VFTableTypeRecord"/> that is read from a PDB image.
/// </summary>
public class SerializedVFTableTypeRecord : VFTableTypeRecord
{
    private readonly PdbReaderContext _context;
    private readonly uint _ownerTypeIndex;
    private readonly uint _baseVFTableIndex;
    private readonly BinaryStreamReader _namesReader;

    /// <summary>
    /// Reads a virtual function table type from the provided input stream.
    /// </summary>
    /// <param name="context">The reading context in which the type is situated in.</param>
    /// <param name="typeIndex">The index to assign to the type.</param>
    /// <param name="reader">The input stream to read from.</param>
    public SerializedVFTableTypeRecord(PdbReaderContext context, uint typeIndex, BinaryStreamReader reader)
        : base(typeIndex)
    {
        _context = context;
        _ownerTypeIndex = reader.ReadUInt32();
        _baseVFTableIndex = reader.ReadUInt32();
        OffsetInObjectLayout = reader.ReadUInt32();
        uint namesLength = reader.ReadUInt32();
        _namesReader = reader.ForkAbsolute(reader.Offset, namesLength);
    }

    /// <inheritdoc />
    protected override CodeViewTypeRecord? GetOwnerType()
    {
        return _context.ParentImage.TryGetLeafRecord(_ownerTypeIndex, out CodeViewTypeRecord? type)
            ? type
            : _context.Parameters.ErrorListener.BadImageAndReturn<CodeViewTypeRecord>(
                $"VFTable type {TypeIndex:X8} contains an invalid owner type index {_ownerTypeIndex:X8}.");
    }

    /// <inheritdoc />
    protected override CodeViewTypeRecord? GetBaseVFTable()
    {
        if (_baseVFTableIndex == 0)
            return null;

        return _context.ParentImage.TryGetLeafRecord(_baseVFTableIndex, out CodeViewTypeRecord? type)
            ? type
            : _context.Parameters.ErrorListener.BadImageAndReturn<CodeViewTypeRecord>(
                $"VFTable type {TypeIndex:X8} contains an invalid base vftable type index {_baseVFTableIndex:X8}.");
    }

    /// <inheritdoc />
    protected override IList<Utf8String> GetNames()
    {
        var reader = _namesReader.Fork();
        var result = new List<Utf8String>();

        while (reader.CanRead(1))
        {
            var name = reader.ReadUtf8String();
            result.Add(name);
        }

        return result;
    }
}
