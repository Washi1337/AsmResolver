namespace AsmResolver.Net
{
    public interface INameProvider
    {
        /// <summary>
        /// Gets the name of the member.
        /// </summary>
        string Name
        {
            get;
        }
    }

    public interface IFullNameProvider : INameProvider
    {
        /// <summary>
        /// Gets the full name of the member.
        /// </summary>
        string FullName
        {
            get;
        }
    }
}