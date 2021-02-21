using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.Workspaces.DotNet;

namespace AsmResolver.Workspaces.Dotnet.Analyzers
{
    internal static class AnalyzerUtilities
    {
        internal static IEnumerable<MethodDefinition> FindBaseMethods(this MethodDefinition subject, WorkspaceIndex index)
        {
            var declaringType = subject.DeclaringType;

            var candidates = index
                .GetOrCreateNode(declaringType) // Get indexed declaring type.
                .GetRelatedObjects(DotNetRelations.BaseType) // Get types that this declaring type is implementing.
                .SelectMany(type => type.Resolve()?.Methods ?? Enumerable.Empty<MethodDefinition>()) // Get the methods.
                .Where(method => method.Name == subject.Name) // Filter on methods with the same name.
                .ToArray();

            var comparer = new SignatureComparer();

            for (int i = 0; i < candidates.Length; i++)
            {
                var candidate = candidates[i];
                if (!candidate.IsVirtual)
                    continue;

                bool isImplementation = candidate.DeclaringType.IsInterface && candidate.IsNewSlot;
                bool isOverride = !candidate.DeclaringType.IsInterface && subject.IsReuseSlot;
                if (!isImplementation && !isOverride)
                    continue;

                if (comparer.Equals(candidate.Signature, subject.Signature))
                    yield return candidate;
            }
        }
    }
}
