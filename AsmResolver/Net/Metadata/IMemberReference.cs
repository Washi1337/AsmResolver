namespace AsmResolver.Net.Metadata
{
    public interface IMemberReference : IFullNameProvider, IHasCustomAttribute
    {
        ITypeDefOrRef DeclaringType
        {
            get;
        }
    }
}