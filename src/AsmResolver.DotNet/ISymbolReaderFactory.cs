using AsmResolver.DotNet.Serialized;

namespace AsmResolver.DotNet;

public interface ISymbolReaderFactory
{
    ISymbolReader CreateSymbolReader(SerializedModuleDefinition module);
}
