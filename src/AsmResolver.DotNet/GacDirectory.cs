using System;
using System.IO;
using System.Linq;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Provides information about a directory in the global assembly cache (GAC).
    /// </summary>
    public struct GacDirectory
    {
        private readonly string _basePath;
        private readonly string _prefix;

        /// <summary>
        /// Creates a new record of a global assembly cache info.
        /// </summary>
        /// <param name="basePath">The path to the global assembly cache.</param>
        /// <param name="prefix">The string to prepend to the assembly's directory.</param>
        public GacDirectory(string basePath, string prefix=null)
        {
            _basePath = basePath ?? throw new ArgumentNullException(nameof(basePath));
            _prefix = prefix;
        }

        private bool IsPrefixed => !string.IsNullOrEmpty(_prefix);

        /// <summary>
        /// Probes the global assembly cache for an assembly.
        /// </summary>
        /// <param name="assembly">The assembly to lookup.</param>
        /// <returns>The path to the assembly, or <c>null</c> if none was found.</returns>
        public string Probe(AssemblyDescriptor assembly)
        {
            string fullPath = Path.Combine(_basePath, assembly.Name);
            if (Directory.Exists(fullPath))
            {
                string pubKeyTokenString = string.Join(string.Empty, 
                    assembly.GetPublicKeyToken().Select(x=>x.ToString("x2")));
                string directoryName = assembly.Version + "__" + pubKeyTokenString;
                if (IsPrefixed)
                    directoryName = _prefix + directoryName;

                string filePath = Path.Combine(fullPath, directoryName, assembly.Name);
                filePath = DotNetFrameworkAssemblyResolver.ProbeFileFromFilePathWithoutExtension(filePath);
                if (!string.IsNullOrEmpty(filePath))
                    return filePath;
            }

            return null;
        }

        /// <inheritdoc />
        public override string ToString() => _basePath;
    }
}