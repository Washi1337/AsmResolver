using System.Collections.Generic;
using AsmResolver.DotNet.PortablePdbs;
using AsmResolver.DotNet.Serialized;

namespace AsmResolver.DotNet;

public interface ISymbolReader
{
    IEnumerable<Document> GetDocuments();
    MethodDebugInformation? GetMethodDebugInformation(SerializedMethodDefinition method);
    IEnumerable<LocalScope> GetLocalScopes(SerializedMethodDefinition method);
    MethodDefinition? GetKickoffMethod(SerializedMethodDefinition method);
    MethodDefinition? GetMoveNextMethod(SerializedMethodDefinition method);
    IEnumerable<CustomDebugInformation> GetCustomDebugInformations(IHasCustomDebugInformation member);
}
