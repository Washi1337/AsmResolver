using System;
using System.Collections.Generic;
using AsmResolver.DotNet.PortablePdbs.Serialized;
using AsmResolver.DotNet.Signatures;
using AsmResolver.IO;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.PortablePdbs.CustomRecords;

public class TypeDefinitionDocumentRecord : CustomDebugRecord
{
    public static Guid KnownKind { get; } = new("932E74BC-DBA9-4478-8D46-0F32A7BAB3D3");

    public override Guid Kind => KnownKind;

    public override bool HasBlob => true;

    public Document[]? Documents { get; set; }

    public static TypeDefinitionDocumentRecord FromReader(PdbReaderContext context, ref BinaryStreamReader reader)
    {
        var documentList = new List<Document>(1);
        while (reader.CanRead(1))
        {
            if (context.Pdb.TryLookupMember<Document>(new MetadataToken(TableIndex.Document, reader.ReadCompressedUInt32()), out var document))
            {
                documentList.Add(document);
            }
        }
        return new TypeDefinitionDocumentRecord
        {
            Documents = documentList.ToArray(),
        };
    }

    protected override void WriteContents(in BlobSerializationContext context)
    {
        throw new NotImplementedException();
    }
}
