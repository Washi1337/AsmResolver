namespace AsmResolver.Net
{
    public interface INameProvider
    {
        string Name
        {
            get;
        }
    }

    public interface IFullNameProvider : INameProvider
    {
        string FullName
        {
            get;
        }
    }
}