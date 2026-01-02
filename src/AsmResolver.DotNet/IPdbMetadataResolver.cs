using AsmResolver.DotNet.PortablePdbs;
using AsmResolver.DotNet.PortablePdbs.Serialized;
using AsmResolver.DotNet.Serialized;

namespace AsmResolver.DotNet
{
    public interface IPdbMetadataResolver
    {
        PdbReaderContext? ResolvePortablePdb(SerializedModuleDefinition module);
    }
}
