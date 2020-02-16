using System.IO;
using System.Runtime.InteropServices;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Provides an implementation of an assembly resolver that includes .NET Core runtime libraries. 
    /// </summary>
    public class NetCoreAssemblyResolver : AssemblyResolverBase
    {
        private readonly string _installationDirectory;
        
        /// <summary>
        /// Creates a new .NET Core assembly resolver, by attempting to autodetect the current .NET Core installation
        /// directory.
        /// </summary>
        public NetCoreAssemblyResolver()
            : this (RuntimeInformation.FrameworkDescription.Contains("Core")
                ? Path.GetDirectoryName(typeof(object).Assembly.Location)
                : null)
        {
        }

        /// <summary>
        /// Creates a new .NET Core assembly resolver.
        /// </summary>
        /// <param name="installationDirectory">The installation directory of .NET Core.</param>
        public NetCoreAssemblyResolver(string installationDirectory)
        {
            _installationDirectory = installationDirectory;
        }

        /// <inheritdoc />
        protected override AssemblyDefinition ResolveImpl(AssemblyDescriptor assembly)
        {
            string path = null;
            
            var token = assembly.GetPublicKeyToken();
            if (token != null && !string.IsNullOrEmpty(_installationDirectory))
                path = ProbeDirectory(assembly, _installationDirectory);
            if (string.IsNullOrEmpty(path))
                path = ProbeSearchDirectories(assembly);

            AssemblyDefinition assemblyDef = null;
            try
            {
                assemblyDef = LoadAssemblyFromFile(path);
            }
            catch
            {
                // ignore any errors.
            }

            return assemblyDef;
        }
    }
}