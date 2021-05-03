using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AsmResolver.IO
{
    public sealed class UncachedFileService : IFileService
    {
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
        public IInputFile OpenFile(string filePath) => new ByteArrayInputFile(filePath, File.ReadAllBytes(filePath), 0);

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
