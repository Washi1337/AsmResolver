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
            using var reader = new StreamReader(stream);

            _template = reader.ReadToEnd();
        }

        protected override string ExecutableExtension => ".dll";

        protected override ProcessStartInfo GetStartInfo(string filePath)
        {
            string deps = _template
                    .Replace("%tfm%", "netcoreapp3.1")
                    .Replace("%version%", "3.1.0")
                    .Replace("%name%", "Microsoft.NETCore.App")
                ;
            
            File.WriteAllText(Path.ChangeExtension(filePath, ".runtimeconfig.json"), deps);
            
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