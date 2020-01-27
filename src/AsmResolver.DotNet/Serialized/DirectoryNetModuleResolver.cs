using System;
using System.IO;

namespace AsmResolver.DotNet.Serialized
{
    /// <summary>
    /// Provides a basic implementation for a net module resolver, that searches for the net module in a directory.  
    /// </summary>
    public class DirectoryNetModuleResolver : INetModuleResolver
    {
        /// <summary>
        /// Creates a new net module resolver that searches for the module in a directory.
        /// </summary>
        /// <param name="directory">The path to the search directory.</param>
        /// <param name="readParameters">The parameters to use for reading a module.</param>
        public DirectoryNetModuleResolver(string directory, ModuleReadParameters readParameters)
        {
            Directory = directory ?? throw new ArgumentNullException(nameof(directory));
            ReadParameters = readParameters;
        }
        
        /// <summary>
        /// Gets the search directory.
        /// </summary>
        public string Directory
        {
            get;
        }

        /// <summary>
        /// Gets the parameters to be used for reading a .NET module.
        /// </summary>
        public ModuleReadParameters ReadParameters
        {
            get;
        }

        /// <inheritdoc />
        public ModuleDefinition Resolve(string name)
        {
            string path = Path.Combine(Directory, name);
            if (File.Exists(path))
            {
                try
                {
                    return ModuleDefinition.FromFile(path, ReadParameters);
                }
                catch
                {
                    // Ignore errors.
                    return null;
                }
            }

            return null;
        }
    }
}