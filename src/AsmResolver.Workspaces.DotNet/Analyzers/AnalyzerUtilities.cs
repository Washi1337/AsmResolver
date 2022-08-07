using System.Linq;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;

namespace AsmResolver.Workspaces.DotNet.Analyzers.Implementation
{
    internal static class AnalyzerUtilities
    {
        private static readonly SignatureComparer _comparer = new ();

        public static AssemblyDescriptor? GetAssembly(IMetadataMember member) => member switch
        {
            AssemblyDescriptor assembly => assembly,
            MemberReference {Parent: { } parent} => GetAssembly(parent),
            ITypeDescriptor {Scope: { } scope} => scope.GetAssembly(),
            IResolutionScope scope => scope.GetAssembly(),
            IModuleProvider {Module: {Assembly: { } assembly}} => assembly,
            _ => null
        };

        public static bool ContainsSubjectAssembly(this Workspace workspace, IMetadataMember member)
            => workspace is DotNetWorkspace dotNetWorkspace
               && GetAssembly(member) is { } assembly
               && dotNetWorkspace.Assemblies
                   .Any(a => _comparer.Equals(a, assembly));
    }
}
