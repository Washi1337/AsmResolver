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

        /// <inheritdoc />
        public void InvalidateFile(string filePath) => _files.TryRemove(filePath, out _);

        /// <inheritdoc />
        void IDisposable.Dispose() => _files.Clear();
    }
}
