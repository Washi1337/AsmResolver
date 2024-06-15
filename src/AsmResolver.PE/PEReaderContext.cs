using System;
using AsmResolver.PE.File;

namespace AsmResolver.PE
{
    /// <summary>
    /// Provides a context in which a PE image reader exists in. This includes the original PE file as
    /// well as reader parameters.
    /// </summary>
    public class PEReaderContext : IErrorListener
    {
        /// <summary>
        /// Creates a new instance of the <see cref="PEReaderContext"/> class.
        /// </summary>
        /// <param name="file">The original PE file.</param>
        public PEReaderContext(PEFile file)
            : this(file, new PEReaderParameters())
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="PEReaderContext"/> class.
        /// </summary>
        /// <param name="file">The original PE file.</param>
        /// <param name="parameters">The reader parameters.</param>
        public PEReaderContext(PEFile file, PEReaderParameters parameters)
        {
            File = file;
            Parameters = parameters;
        }

        /// <summary>
        /// Gets the original PE file that is being parsed.
        /// </summary>
        public PEFile File
        {
            get;
        }

        /// <summary>
        /// Gets the reader parameters to use while parsing the PE file.
        /// </summary>
        public PEReaderParameters Parameters
        {
            get;
        }

        /// <summary>
        /// Creates relocation parameters based on the current PE file that is being read.
        /// </summary>
        /// <param name="offset">The offset of the segment.</param>
        /// <param name="rva">The relative virtual address of the segment.</param>
        /// <returns>The created relocation parameters.</returns>
        public RelocationParameters GetRelocation(ulong offset, uint rva)
        {
            return new RelocationParameters(File.OptionalHeader.ImageBase, offset, rva,
                File.OptionalHeader.Magic == OptionalHeaderMagic.PE32);
        }

        /// <inheritdoc />
        public void MarkAsFatal() => Parameters.ErrorListener.MarkAsFatal();

        /// <inheritdoc />
        public void RegisterException(Exception exception) => Parameters.ErrorListener.RegisterException(exception);
    }
}
