using AsmResolver.Net.Metadata;

namespace AsmResolver.Net
{
    public interface ITypeDescriptor : IFullNameProvider
    {
        string Namespace
        {
            get;
        }

        ITypeDescriptor DeclaringType
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