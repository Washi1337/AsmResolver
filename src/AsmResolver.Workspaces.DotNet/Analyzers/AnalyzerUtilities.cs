using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;

namespace AsmResolver.Workspaces.DotNet.Analyzers
{
    internal static class AnalyzerUtilities
    {
        internal static IEnumerable<MethodDefinition> FindBaseMethods(this MethodDefinition subject, WorkspaceIndex index)
        {
            var declaringType = subject.DeclaringType;

            var baseTypes = index
                .GetOrCreateNode(declaringType) // Get indexed declaring type.
                .GetRelatedObjects(DotNetRelations.BaseType) // Get types that this declaring type is implementing.
                .ToArray();
            
            var candidates = baseTypes
                .SelectMany(type => type.Resolve()?.Methods ?? Enumerable.Empty<MethodDefinition>()) // Get the methods.
                .Where(method => method.Name == subject.Name) // Filter on methods with the same name.
                .ToArray();


            var genericContexts = new List<GenericContext>();
            foreach (var relatedObject in baseTypes)
            {
                if (relatedObject is not TypeSpecification {Signature: GenericInstanceTypeSignature genericSignature})
                    continue;
                var context = new GenericContext(genericSignature, null);
                genericContexts.Add(context);
            }

            var comparer = new SignatureComparer();

            var signature = subject.Signature;
            foreach (var t in genericContexts)
                signature = signature.InstantiateGenericTypes(t);
            
            for (int i = 0; i < candidates.Length; i++)
            {
                var candidate = candidates[i];
                if (!candidate.IsVirtual)
                    continue;

                bool isImplementation = candidate.DeclaringType.IsInterface && candidate.IsNewSlot;
                bool isOverride = !candidate.DeclaringType.IsInterface && subject.IsReuseSlot;
                if (!isImplementation && !isOverride)
                    continue;

                var candidateSignature = candidate.Signature;
                foreach (var t in genericContexts)
                    candidateSignature = candidateSignature.InstantiateGenericTypes(t);

                if (comparer.Equals(candidateSignature, signature))
                    yield return candidate;
            }
        }
    }
}
