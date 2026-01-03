using System;

namespace AsmResolver.DotNet.PortablePdbs;

public static class KnownHashAlgorithm
{
    public static Guid SHA1 { get; } = new("ff1816ec-aa5e-4d10-87f7-6f4963833460");
    public static Guid SHA256 { get; } = new("8829d00f-11b8-4213-878b-770e8597ac16");
}