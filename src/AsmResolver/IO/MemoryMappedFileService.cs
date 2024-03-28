#if !NET35

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace AsmResolver.IO
{
    /// <summary>
    /// Provides an implementation of the <see cref="IFileService"/> interface, which maps any requested file into
    /// memory, and keeps track of any of the instances it creates.
    /// </summary>
    public class MemoryMappedFileService : IFileService
    {
        private readonly ConcurrentDictionary<string, MemoryMappedInputFile> _files = new();

        /// <inheritdoc />
        public IEnumerable<string> GetOpenedFiles() => _files.Keys;

        /// <inheritdoc />
        public IInputFile OpenFile(string filePath)
        {
            return _files.GetOrAdd(filePath, x => new MemoryMappedInputFile(x));
        }

        /// <inheritdoc />
        public void InvalidateFile(string filePath)
        {
            if (_files.TryRemove(filePath, out var file))
                file.Dispose();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            foreach (var file in _files.Values)
                file.Dispose();
            _files.Clear();
        }
    }
}

#endif
