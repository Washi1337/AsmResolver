using System.Collections.Generic;
using AsmResolver.DotNet.PortablePdbs;
using AsmResolver.DotNet.Serialized;

namespace AsmResolver.DotNet;

public class NullSymbolReader : ISymbolReader
{
    public static NullSymbolReader Instance { get; } = new();

    public IEnumerable<Document> GetDocuments() => [];

    public MethodDebugInformation? GetMethodDebugInformation(SerializedMethodDefinition method) => null;

    public IEnumerable<LocalScope> GetLocalScopes(SerializedMethodDefinition method) => [];

    public MethodDefinition? GetKickoffMethod(SerializedMethodDefinition method) => null;

    public MethodDefinition? GetMoveNextMethod(SerializedMethodDefinition method) => null;

    public IEnumerable<CustomDebugInformation> GetCustomDebugInformations(IHasCustomDebugInformation member) => [];
}
