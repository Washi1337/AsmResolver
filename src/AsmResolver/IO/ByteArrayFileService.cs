using System;
using System.Collections.Generic;
using System.IO;

namespace AsmResolver.IO
{
    public class ByteArrayFileService : IFileService
    {
        private readonly Dictionary<string, ByteArrayInputFile> _files = new();

        /// <inheritdoc />
        public IEnumerable<string> GetOpenedFiles() => _files.Keys;

        /// <inheritdoc />
        public IInputFile OpenFile(string filePath)
        {
            if (!_files.TryGetValue(filePath, out var file))
            {
                file = new ByteArrayInputFile(filePath, File.ReadAllBytes(filePath), 0);
                _files.Add(filePath, file);
            }

            return file;
        }

        /// <inheritdoc />
        public void InvalidateFile(string filePath) => _files.Remove(filePath);

        /// <inheritdoc />
        void IDisposable.Dispose() => _files.Clear();
    }
}
