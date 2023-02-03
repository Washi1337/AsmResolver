using System;
using System.Collections.Generic;
using System.IO;
using AsmResolver.IO;

namespace AsmResolver.Symbols.Pdb.Leaves.Serialized;

public class SerializedBuildInfoLeaf : BuildInfoLeaf
{
    private readonly PdbReaderContext _context;
    private readonly BinaryStreamReader _reader;

    public SerializedBuildInfoLeaf(PdbReaderContext context, uint typeIndex, BinaryStreamReader reader)
        : base(typeIndex)
    {
        _context = context;
        _reader = reader;


    }

    protected override IList<string> GetEntries()
    {
        var result = new List<string>();

        var reader = _reader.Fork();
        uint count = reader.ReadUInt32();

        for (int i = 0; i < count; i++)
        {
            uint index = reader.ReadUInt32();
            if (!_context.ParentImage.TryGetLeafRecord(index, out var leaf))
            {

            }
        }

        return result;
    }
}
