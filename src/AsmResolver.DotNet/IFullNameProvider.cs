namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a named object that has a full name.
    /// </summary>
    public interface IFullNameProvider : INameProvider
    {
        /// <summary>
        /// Gets the full name of the object.
        /// </summary>
        string FullName
        {
            get;
        }
    }
}
