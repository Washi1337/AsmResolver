using System.Collections.Generic;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;

namespace AsmResolver.DotNet.Extensions.Memory
{
    internal sealed class CycleDetectionVisitor : IVisitor
    {
        private readonly HashSet<TypeSignature> _visited = new HashSet<TypeSignature>(new SignatureComparer());

        public void VisitComplex(FieldNode node)
        {
            if (!_visited.Add(node.Signature))
                ThrowCycleException();
            
            foreach (var child in node.Children)
                child.Accept(this);
        }

        public void VisitPrimitive(FieldNode node)
        {
            if (!_visited.Add(node.Signature))
                ThrowCycleException();
        }

        private static void ThrowCycleException()
        {
            throw new TypeMemoryLayoutDetectionException("Cyclic dependency in struct");
        }
    }
}