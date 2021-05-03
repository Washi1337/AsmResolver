using System;
using System.Collections.Generic;

namespace AsmResolver.IO
{
    public interface IFileService : IDisposable
    {
        IEnumerable<string> GetOpenedFiles();

        IInputFile OpenFile(string filePath);

        void InvalidateFile(string filePath);
    }
}
