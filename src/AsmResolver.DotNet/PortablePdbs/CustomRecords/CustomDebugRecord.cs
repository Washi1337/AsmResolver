using System;
using AsmResolver.DotNet.Signatures;

namespace AsmResolver.DotNet.PortablePdbs.CustomRecords;

public abstract class CustomDebugRecord : ExtendableBlobSignature
{
    public abstract Guid Kind
    {
        get;
    }
}