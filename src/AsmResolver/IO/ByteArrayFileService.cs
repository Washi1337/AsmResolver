using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace AsmResolver.IO
{
    /// <summary>
    /// Provides an implementation of a <see cref="IFileService"/> that uses instances of
    /// <see cref="ByteArrayInputFile"/> to represent opened files, and keeps track of any of the instances
    /// it creates.
    /// </summary>
    public class ByteArrayFileService : IFileService
    {
        private readonly ConcurrentDictionary<string, ByteArrayInputFile> _files = new();

        /// <inheritdoc />
        public IEnumerable<string> GetOpenedFiles() => _files.Keys;

        /// <inheritdoc />
        public IInputFile OpenFile(string filePath)
        {
            return _files.GetOrAdd(filePath, x => new ByteArrayInputFile(x));
        }

        /// <summary>
        /// Assigns a file path to a byte array and opens it.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="contents">The contents of the file.</param>
        /// <returns>The opened file.</returns>
        public IInputFile OpenBytesAsFile(string filePath, byte[] contents)
        {
            return _files.GetOrAdd(filePath, p => new ByteArrayInputFile(p, contents, 0));
        }

        /// <inheritdoc />
        public void InvalidateFile(string filePath) => _files.TryRemove(filePath, out _);

        /// <inheritdoc />
        void IDisposable.Dispose() => _files.Clear();
    }
}
