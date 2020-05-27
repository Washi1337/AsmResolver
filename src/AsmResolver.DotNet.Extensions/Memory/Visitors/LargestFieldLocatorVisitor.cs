using System;

namespace AsmResolver.DotNet.Extensions.Memory.Visitors
{
    internal sealed class LargestFieldLocatorVisitor : IVisitor
    {
        private readonly bool _is32Bit;

        internal LargestFieldLocatorVisitor(bool is32Bit)
        {
            _is32Bit = is32Bit;
        }
        
        internal uint Largest
        {
            get;
            private set;
        }
        
        public void VisitComplex(FieldNode node)
        {
            foreach (var child in node.Children)
                child.Accept(this);
        }

        public void VisitPrimitive(FieldNode node)
        {
            Largest = Math.Max(Largest, node.Signature.SizeInBytes(_is32Bit));
        }
    }
}