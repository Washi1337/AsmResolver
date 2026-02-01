using System;
using AsmResolver.DotNet.PortablePdbs.Serialized;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.PortablePdbs.CustomRecords;

public abstract class CustomDebugRecord : ExtendableBlobSignature
{
    public abstract Guid Kind
    {
        get;
    }

    public abstract bool HasBlob
    {
        get;
    }

    public static CustomDebugRecord FromRow(PdbReaderContext context, in CustomDebugInformationRow row)
    {
        var kind = context.GuidStream!.GetGuidByIndex(row.Kind);
        var blobReaderContext = new BlobReaderContext(context.OwningModule.ReaderContext);
        var hasBlob = context.BlobStream!.TryGetBlobReaderByIndex(row.Value, out var reader);

        if (kind == PrimaryConstructorInformationRecord.KnownKind)
        {
            return new PrimaryConstructorInformationRecord();
        }
        else if (kind == EmbeddedSourceRecord.KnownKind)
        {
            return EmbeddedSourceRecord.FromReader(blobReaderContext, ref reader);
        }
        else if (kind == SourceLinkRecord.KnownKind)
        {
            return SourceLinkRecord.FromReader(blobReaderContext, ref reader);
        }
        else if (kind == DefaultNamespaceRecord.KnownKind)
        {
            return DefaultNamespaceRecord.FromReader(blobReaderContext, ref reader);
        }
        else if (kind == DynamicLocalVariablesRecord.KnownKind)
        {
            return DynamicLocalVariablesRecord.FromReader(blobReaderContext, ref reader);
        }

        if (hasBlob)
        {
            return new UnknownDebugRecord(kind, reader.ReadToEnd());
        }
        else
        {
            return new UnknownDebugRecord(kind, null);
        }
    }
}
