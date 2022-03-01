using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;

namespace AsmResolver.Workspaces.DotNet.Analyzers
{
    internal static class AnalyzerUtilities
    {
        private static readonly SignatureComparer _comparer = new ();

        internal static IEnumerable<MethodDefinition> FindBaseMethods(this MethodDefinition subject,
            WorkspaceIndex index)
        {
            if (subject.DeclaringType is not { } declaringType)
                yield break;

            var baseTypes = index
                .GetOrCreateNode(declaringType) // Get indexed declaring type.
                .ForwardRelations.GetObjects(DotNetRelations.BaseType) // Get types that this declaring type is implementing.
                .ToArray();

            foreach (var baseType in baseTypes)
            {
                if (baseType.Resolve() is not {} type)
                    continue;

                var candidates = type.Methods
                    .Where(m => m.Name == subject.Name)
                    .ToArray();

                if (!candidates.Any())
                    continue;

                GenericContext? context = null;
                if (baseType is TypeSpecification {Signature: GenericInstanceTypeSignature genericSignature})
                    context = new GenericContext(genericSignature, null);

                foreach (var candidate in candidates)
                {
                    if (!candidate.IsVirtual || candidate.DeclaringType is null)
                        continue;

                    bool isImplementation = candidate.DeclaringType.IsInterface && candidate.IsNewSlot;
                    bool isOverride = !candidate.DeclaringType.IsInterface && subject.IsReuseSlot;
                    if (!isImplementation && !isOverride)
                        continue;
                    var signature = candidate.Signature;
                    if (signature is not null && context.HasValue)
                        signature = signature.InstantiateGenericTypes(context.Value);

                    if (_comparer.Equals(signature, subject.Signature))
                        yield return candidate;
                }
            }
        }

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
