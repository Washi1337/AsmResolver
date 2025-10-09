using AsmResolver.DotNet.PortablePdbs;
using AsmResolver.DotNet.PortablePdbs.Serialized;

namespace AsmResolver.DotNet
{
    public interface IPdbMetadataResolver
    {
        PdbReaderContext? ResolvePortablePdb(ModuleDefinition module);
    }
}
