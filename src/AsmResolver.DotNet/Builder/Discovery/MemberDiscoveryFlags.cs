using System;

namespace AsmResolver.DotNet.Builder.Discovery
{
    /// <summary>
    /// Provides members describing the types of members for which the order in which they are defined in the original
    /// module can be preserved.
    /// </summary>
    [Flags]
    public enum MemberDiscoveryFlags
    {
        /// <summary>
        /// Specifies no list of definitions need to be preserved in order. 
        /// </summary>
        None = 0,
        
        /// <summary>
        /// Specifies the list of type definitions needs to be preserved in order.
        /// </summary>
        PreserveTypeOrder = 1,
        
        /// <summary>
        /// Specifies the list of field definitions needs to be preserved in order.
        /// </summary>
        PreserveFieldOrder = 2,
        
        /// <summary>
        /// Specifies the list of method definitions needs to be preserved in order.
        /// </summary>
        PreserveMethodOrder = 4,
        
        /// <summary>
        /// Specifies the list of parameter definitions needs to be preserved in order.
        /// </summary>
        PreserveParameterOrder = 8,
        
        /// <summary>
        /// Specifies the list of event definitions needs to be preserved in order.
        /// </summary>
        PreserveEventOrder = 16,
        
        /// <summary>
        /// Specifies the list of properties definitions needs to be preserved in order.
        /// </summary>
        PreservePropertyOrder = 32
    }
}