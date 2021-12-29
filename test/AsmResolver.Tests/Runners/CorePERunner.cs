using System;
using System.Diagnostics;
using System.IO;

namespace AsmResolver.Tests.Runners
{
    public class CorePERunner : PERunner
    {
        private readonly string _template;

        public CorePERunner(string basePath)
            : base(basePath)
        {
            using var stream = typeof(CorePERunner).Assembly.GetManifestResourceStream(
                "AsmResolver.Tests.Resources.RuntimeConfigTemplate.txt");

            if (stream is null)
            {
                throw new ArgumentException(
                    "RuntimeConfigTemplate.txt not found. Test library might not be built correctly.");
            }

            using var reader = new StreamReader(stream);

            _template = reader.ReadToEnd();
        }

        protected override string ExecutableExtension => ".dll";

        protected override ProcessStartInfo GetStartInfo(string filePath, string[]? arguments)
        {
            string deps = _template
                    .Replace("%tfm%", "netcoreapp3.1")
                    .Replace("%version%", "3.1.0")
                    .Replace("%name%", "Microsoft.NETCore.App")
                ;

            File.WriteAllText(Path.ChangeExtension(filePath, ".runtimeconfig.json"), deps);

            var result = new ProcessStartInfo
            {
                FileName = "dotnet",
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            result.ArgumentList.Add(filePath);
            if (arguments is not null)
            {
                foreach (string argument in arguments)
                    result.ArgumentList.Add(argument);
            }

            return result;
        }
    }
}
