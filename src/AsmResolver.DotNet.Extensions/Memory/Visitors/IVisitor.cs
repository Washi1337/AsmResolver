namespace AsmResolver.DotNet.Extensions.Memory.Visitors
{
    internal interface IVisitor
    {
        void VisitComplex(FieldNode node);
        void VisitPrimitive(FieldNode node);
    }
}