namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents an object that has a name.
    /// </summary>
    public interface INameProvider
    {
        /// <summary>
        /// Gets the name of the object.
        /// </summary>
        string Name
        {
            get;
        }
    }

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