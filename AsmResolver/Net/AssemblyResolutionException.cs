using System;

namespace AsmResolver.Net
{
    public class AssemblyResolutionException : Exception
    {
        public AssemblyResolutionException()
        {
        }

        public AssemblyResolutionException(string message)
            : base(message)
        {
        }

        public AssemblyResolutionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public AssemblyResolutionException(IAssemblyDescriptor info)
            : this(string.Format("Assembly {0} could not be resolved.", info.GetFullName()))
        {
        }

        public IAssemblyDescriptor RequestedAssembly
        {
            get;
            set;
        }
    }
}