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
        var hasBlob = context.BlobStream!.TryGetBlobReaderByIndex(row.Value, out var reader);

        if (kind == PrimaryConstructorInformationRecord.KnownKind)
        {
            return new PrimaryConstructorInformationRecord();
        }
        else if (kind == EmbeddedSourceRecord.KnownKind)
        {
            return EmbeddedSourceRecord.FromReader(context, ref reader);
        }
        else if (kind == SourceLinkRecord.KnownKind)
        {
            return SourceLinkRecord.FromReader(context, ref reader);
        }
        else if (kind == DefaultNamespaceRecord.KnownKind)
        {
            return DefaultNamespaceRecord.FromReader(context, ref reader);
        }
        else if (kind == DynamicLocalVariablesRecord.KnownKind)
        {
            return DynamicLocalVariablesRecord.FromReader(context, ref reader);
        }
        else if (kind == EnCLocalSlotMapRecord.KnownKind)
        {
            return EnCLocalSlotMapRecord.FromReader(context, ref reader);
        }
        else if (kind == EnCLambdaClosureMapRecord.KnownKind)
        {
            return EnCLambdaClosureMapRecord.FromReader(context, ref reader);
        }
        else if (kind == TupleElementNamesRecord.KnownGuid)
        {
            return TupleElementNamesRecord.FromReader(context, ref reader);
        }
        else if (kind == CompilationOptionsRecord.KnownKind)
        {
            return CompilationOptionsRecord.FromReader(context, ref reader);
        }
        else if (kind == CompilationMetadataReferencesRecord.KnownKind)
        {
            return CompilationMetadataReferencesRecord.FromReader(context, ref reader);
        }
        else if (kind == TypeDefinitionDocumentRecord.KnownKind)
        {
            return TypeDefinitionDocumentRecord.FromReader(context, ref reader);
        }
        else if (kind == StateMachineHoistedLocalScopesRecord.KnownKind)
        {
            return StateMachineHoistedLocalScopesRecord.FromReader(context, ref reader);
        }
        else if (kind == AsyncMethodSteppingInformationRecord.KnownKind)
        {
            return AsyncMethodSteppingInformationRecord.FromReader(context, ref reader);
        }
        else if (kind == EnCStateMachineStateMapRecord.KnownKind)
        {
            return EnCStateMachineStateMapRecord.FromReader(context, ref reader);
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
