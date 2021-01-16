using System;
using System.Runtime.CompilerServices;
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
        public PEReaderContext(IPEFile file)
            : this(file, new PEReaderParameters())
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="PEReaderContext"/> class.
        /// </summary>
        /// <param name="file">The original PE file.</param>
        /// <param name="parameters">The reader parameters.</param>
        public PEReaderContext(IPEFile file, PEReaderParameters parameters)
        {
            File = file;
            Parameters = parameters;
        }
        
        /// <summary>
        /// Gets the original PE file that is being parsed.
        /// </summary>
        public IPEFile File
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

        /// <inheritdoc />
        public void MarkAsFatal() => Parameters.ErrorListener.MarkAsFatal();

        /// <inheritdoc />
        public void RegisterException(Exception exception) => Parameters.ErrorListener.RegisterException(exception);
    }
}