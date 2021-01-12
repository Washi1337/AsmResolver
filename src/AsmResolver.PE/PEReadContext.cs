using System;
using AsmResolver.PE.File;

namespace AsmResolver.PE
{
    public class PEReadContext
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
    }
}