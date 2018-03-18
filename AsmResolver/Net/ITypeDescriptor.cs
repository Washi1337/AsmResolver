using AsmResolver.Net.Cts;

namespace AsmResolver.Net
{
    public interface ITypeDescriptor : IFullNameProvider
    {
        string Namespace
        {
            get;
        }

        ITypeDescriptor DeclaringTypeDescriptor
        {
            get;
        }

        IResolutionScope ResolutionScope
        {
            get;
        }

        bool IsValueType
        {
            get;
        }

        ITypeDescriptor GetElementType();
    }

}