namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a member definition or reference that resides in a .NET module.
    /// </summary>
    public interface IModuleProvider
    {
        /// <summary>
        /// Gets the module that defines the member definition or reference.
        /// </summary>
        /// <remarks>
        /// For member references, this does not obtain the module definition that the member is defined in. 
        /// Rather, it obtains the module definition that references this reference.
        /// </remarks>
        ModuleDefinition Module
        {
            get;
        }
    }
}