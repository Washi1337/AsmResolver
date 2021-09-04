using AsmResolver.PE.DotNet.Metadata.Strings;

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
        string? Name
        {
            get;
        }
    }
}
