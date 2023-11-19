using System;
using System.Collections.Generic;
using System.Linq;

namespace AsmResolver.IO
{
    /// <summary>
    /// Provides a basic singleton implementation of a <see cref="IFileService"/> that produces instances of the
    /// <see cref="ByteArrayInputFile"/> class, and does no tracking and caching of any of the opened files.
    /// </summary>
    public sealed class UncachedFileService : IFileService
    {
        /// <summary>
        /// Gets the singleton instance of the file service.
        /// </summary>
        public static UncachedFileService Instance
        {
            get;
        } = new();

        private UncachedFileService()
        {
        }

        /// <inheritdoc />
        IEnumerable<string> IFileService.GetOpenedFiles() => Enumerable.Empty<string>();

        /// <inheritdoc />
        public IInputFile OpenFile(string filePath) => new ByteArrayInputFile(filePath);

        /// <inheritdoc />
        void IFileService.InvalidateFile(string filePath)
        {
        }

        /// <inheritdoc />
        void IDisposable.Dispose()
        {
        }
    }
}
