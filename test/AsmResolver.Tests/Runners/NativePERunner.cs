using System.Diagnostics;

namespace AsmResolver.Tests.Runners
{
    public class NativePERunner : PERunner
    {
        public NativePERunner(string basePath) : base(basePath)
        {
        }

        protected override string ExecutableExtension => ".exe";

        protected override ProcessStartInfo GetStartInfo(string filePath)
        {
            return new ProcessStartInfo
            {
                FileName = filePath,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
        }
    }
}