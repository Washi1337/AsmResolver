using System.Diagnostics;

namespace AsmResolver.Tests.Runners
{
    public class CorePERunner : PERunner
    {
        public CorePERunner(string basePath) 
            : base(basePath)
        {
        }

        protected override string ExecutableExtension => ".dll";

        protected override ProcessStartInfo GetStartInfo(string filePath)
        {
            return new ProcessStartInfo
            {
                FileName = "dotnet",
                ArgumentList = { filePath },
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
        }
    }
}