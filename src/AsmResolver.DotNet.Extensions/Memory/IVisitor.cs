namespace AsmResolver.DotNet.Extensions.Memory
{
    internal interface IVisitor
    {
        void VisitComplex(FieldNode node);
        void VisitPrimitive(FieldNode node);
    }
}