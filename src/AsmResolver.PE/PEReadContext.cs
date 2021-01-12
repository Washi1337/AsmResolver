using System;
using System.Runtime.CompilerServices;
using AsmResolver.PE.File;

namespace AsmResolver.PE
{
    public class PEReadContext : IErrorListener
    {
        public PEReadContext(IPEFile file)
            : this(file, new PEReadParameters())
        {
        }

        public PEReadContext(IPEFile file, PEReadParameters parameters)
        {
            File = file;
            Parameters = parameters;
        }
        
        public IPEFile File
        {
            get;
        }

        public PEReadParameters Parameters
        {
            get;
        }

        /// <inheritdoc />
        public void MarkAsFatal() => Parameters.ErrorListener.MarkAsFatal();

        /// <inheritdoc />
        public void RegisterException(Exception exception) => Parameters.ErrorListener.RegisterException(exception);
    }
}