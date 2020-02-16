using System.Reflection;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet
{
    public class ReflectionAssemblyDescriptor : AssemblyDescriptor
    {
        private readonly ModuleDefinition _parentModule;
        private readonly AssemblyName _assemblyName;

        public ReflectionAssemblyDescriptor(ModuleDefinition parentModule, AssemblyName assemblyName)
            : base(new MetadataToken(TableIndex.AssemblyRef, 0))
        {
            _parentModule = parentModule;
            _assemblyName = assemblyName;
        }

        protected override string GetName() =>
            _assemblyName.Name;

        protected override string GetCulture() =>
            _assemblyName.CultureName;
        
        public override byte[] GetPublicKeyToken() => 
            _assemblyName.GetPublicKeyToken();

        public override AssemblyDefinition Resolve() => 
            _parentModule.MetadataResolver.AssemblyResolver.Resolve(this);
    }
}