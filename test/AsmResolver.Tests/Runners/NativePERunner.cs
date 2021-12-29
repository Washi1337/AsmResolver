using System.Diagnostics;

namespace AsmResolver.Tests.Runners
{
    public class NativePERunner : PERunner
    {
        public NativePERunner(string basePath) : base(basePath)
        {
        }

        protected override string ExecutableExtension => ".exe";

        protected override ProcessStartInfo GetStartInfo(string filePath, string[]? arguments)
        {
            var info = new ProcessStartInfo
            {
                FileName = filePath,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            if (arguments is not null)
            {
                foreach (string argument in arguments)
                    info.ArgumentList.Add(argument);
            }

            return info;
        }
    }
}
