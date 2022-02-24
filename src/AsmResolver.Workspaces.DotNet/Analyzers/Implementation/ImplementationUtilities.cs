using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;

namespace AsmResolver.Workspaces.DotNet.Analyzers.Implementation
{
    internal static class ImplementationUtilities
    {
        private static readonly SignatureComparer _comparer = new ();

        internal static IEnumerable<MethodDefinition> FindBaseMethods(this MethodDefinition subject,
            WorkspaceIndex index)
        {
            if (subject.DeclaringType is not { } declaringType)
                yield break;

            var declaringTypeNode = index.GetOrCreateNode(declaringType);
            var baseTypes = GetBaseTypes(declaringTypeNode);

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

        internal static IEnumerable<ITypeDefOrRef> GetBaseTypes(WorkspaceIndexNode baseNode)
        {
            var visited = new HashSet<WorkspaceIndexNode>();
            var agenda = new Queue<WorkspaceIndexNode>();
            agenda.Enqueue(baseNode);
            while (agenda.Count != 0)
            {
                var node = agenda.Dequeue();
                if (!visited.Add(node))
                    continue;
                var baseTypeNodes = node.ForwardRelations.GetNodes(DotNetRelations.BaseType);
                foreach (var baseTypeNode in baseTypeNodes)
                {
                    var baseType = (ITypeDefOrRef)baseTypeNode.Subject;
                    agenda.Enqueue(baseTypeNode);
                    var baseTypeDefinitions = baseTypeNode.BackwardRelations.GetNodes(DotNetRelations.ReferenceType);
                    foreach (var baseTypeDefinition in baseTypeDefinitions)
                        agenda.Enqueue(baseTypeDefinition);
                    yield return baseType;
                }
            }
        }
    }
}
