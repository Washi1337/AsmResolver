#if !NET6_0_OR_GREATER
namespace System.Diagnostics.CodeAnalysis
{
    /// <summary>
    /// Indicates that the specified method requires dynamic access to code that is not
    /// referenced statically, for example, through System.Reflection.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Constructor | AttributeTargets.Method, Inherited = false)]
    internal sealed class RequiresUnreferencedCodeAttribute : Attribute
    {
        /// <summary>
        /// Gets a message that contains information about the usage of unreferenced code.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets or sets an optional URL that contains more information about the method,
        /// why it requires unreferenced code, and what options a consumer has to deal with
        /// it.
        /// </summary>
        public string? Url { get; set; }
 
        /// <summary>
        /// Initializes a new instance of the System.Diagnostics.CodeAnalysis.RequiresUnreferencedCodeAttribute
        /// class with the specified message.
        /// </summary>
        /// <param name="message">A message that contains information about the usage of unreferenced code.</param>
        public RequiresUnreferencedCodeAttribute(string message)
        {
            Message = message;
        }
    }
}
#endif
