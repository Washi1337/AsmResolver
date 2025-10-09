using AsmResolver.DotNet.PortablePdbs;

namespace AsmResolver.DotNet
{
    public interface IPdbMetadataResolver
    {
        PortablePdb? ResolvePortablePdb(ModuleDefinition module);
    }
}
