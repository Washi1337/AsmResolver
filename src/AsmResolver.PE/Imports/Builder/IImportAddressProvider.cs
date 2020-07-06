using System;

namespace AsmResolver.PE.Imports.Builder
{
    /// <summary>
    /// Defines members for resolving thunk RVAs to members in the import directory.
    /// </summary>
    public interface IImportAddressProvider
    {
        /// <summary>
        /// Obtains the RVA of a thunk.
        /// </summary>
        /// <param name="module">The name of the module that defines the imported member.</param>
        /// <param name="member">The name of the imported member.</param>
        /// <returns>The RVA</returns>
        /// <exception cref="ArgumentException">Occurs when the member could not be found.</exception>
        uint GetThunkRva(string module, string member);
    }
}