using System;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Provides members for describing a .NET assembly.
    /// </summary>
    public interface IAssemblyName : INameProvider
    {
        /// <summary>
        /// Gets the version of the referenced assembly.
        /// </summary>
        Version Version
        {
            get;
        }

        /// <summary>
        /// Gets the attributes associated to the referenced assembly.
        /// </summary>
        AssemblyAttributes Attributes
        {
            get;
        }

        /// <summary>
        /// Gets the locale string of the assembly (if available).
        /// </summary>
        string Culture
        {
            get;
        }
        
        /// <summary>
        /// When the application is signed with a strong name, obtains the public key token of the assembly 
        /// </summary>
        /// <returns>The token.</returns>
        byte[] GetPublicKeyToken();
    }
}