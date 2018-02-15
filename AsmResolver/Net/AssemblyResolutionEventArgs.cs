using System;
using AsmResolver.Net.Cts;

namespace AsmResolver.Net
{
    public delegate AssemblyDefinition AssemblyResolutionEventHandler(object sender, AssemblyResolutionEventArgs args);

    public class AssemblyResolutionEventArgs : EventArgs
    {
        public AssemblyResolutionEventArgs(IAssemblyDescriptor requestedAssembly)
        {
            if (requestedAssembly == null)
                throw new ArgumentNullException("requestedAssembly");
            RequestedAssembly = requestedAssembly;
        }

        public AssemblyResolutionEventArgs(IAssemblyDescriptor requestedAssembly, Exception exception)
        {
            if (requestedAssembly == null)
                throw new ArgumentNullException("requestedAssembly");
            RequestedAssembly = requestedAssembly;
            Exception = exception;
        }

        public IAssemblyDescriptor RequestedAssembly
        {
            get;
            set;
        }

        public Exception Exception
        {
            get;
            set;
        }
    }
}