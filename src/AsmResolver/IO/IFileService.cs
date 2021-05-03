using System;
using System.Collections.Generic;

namespace AsmResolver.IO
{
    /// <summary>
    /// Provides members for opening and managing files for reading.
    /// </summary>
    /// <remarks>
    /// The file service is the owner of any of the <see cref="IInputFile"/> objects it produces. Disposing an
    /// instance of this interface results in all files opened by this service to be closed and disposed as well.
    /// </remarks>
    public interface IFileService : IDisposable
    {
        /// <summary>
        /// Gets a collection of files currently opened by this file service.
        /// </summary>
        /// <returns>The paths of the files that were opened.</returns>
        IEnumerable<string> GetOpenedFiles();

        /// <summary>
        /// Opens a file at the provided file path.
        /// </summary>
        /// <param name="filePath">The path to the file to open.</param>
        /// <returns>The opened file.</returns>
        IInputFile OpenFile(string filePath);

        /// <summary>
        /// If the provided file path was opened by this file service, closes the provided file and removes it from
        /// the cache.
        /// </summary>
        /// <param name="filePath">The path of the file to close.</param>
        void InvalidateFile(string filePath);
    }
}
