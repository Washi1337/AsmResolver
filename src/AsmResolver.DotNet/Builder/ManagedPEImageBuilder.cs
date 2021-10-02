using System;
using AsmResolver.DotNet.Code.Native;
using AsmResolver.PE;

namespace AsmResolver.DotNet.Builder
{
    /// <summary>
    /// Provides a default implementation of <see cref="IPEImageBuilder"/>.
    /// </summary>
    public class ManagedPEImageBuilder : IPEImageBuilder
    {
        /// <summary>
        /// Creates a new instance of the <see cref="ManagedPEImageBuilder"/> class, using the default  implementation
        /// of the <see cref="IDotNetDirectoryFactory"/>.
        /// </summary>
        public ManagedPEImageBuilder()
            : this(new DotNetDirectoryFactory())
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ManagedPEImageBuilder"/> class, and initializes a new
        /// .NET data directory factory using the provided metadata builder flags.
        /// </summary>
        public ManagedPEImageBuilder(MetadataBuilderFlags metadataBuilderFlags)
            : this(new DotNetDirectoryFactory(metadataBuilderFlags))
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ManagedPEImageBuilder"/> class, using the provided
        /// .NET data directory flags.
        /// </summary>
        public ManagedPEImageBuilder(IDotNetDirectoryFactory factory)
        {
            DotNetDirectoryFactory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        /// <summary>
        /// Gets or sets the factory responsible for constructing the .NET data directory.
        /// </summary>
        public IDotNetDirectoryFactory DotNetDirectoryFactory
        {
            get;
            set;
        }

        /// <inheritdoc />
        public PEImageBuildResult CreateImage(ModuleDefinition module)
        {
            var context = new PEImageBuildContext();

            PEImage? image = null;
            ITokenMapping? tokenMapping = null;

            try
            {
                // Create basic PE image skeleton.
                image = new PEImage
                {
                    MachineType = module.MachineType,
                    PEKind = module.PEKind,
                    Characteristics = module.FileCharacteristics,
                    SubSystem = module.SubSystem,
                    DllCharacteristics = module.DllCharacteristics,
                    Resources = module.NativeResourceDirectory,
                    TimeDateStamp = module.TimeDateStamp
                };

                // Construct new .NET directory.
                var symbolProvider = new NativeSymbolsProvider(image.ImageBase);
                var result = DotNetDirectoryFactory.CreateDotNetDirectory(
                    module,
                    symbolProvider,
                    context.DiagnosticBag);
                image.DotNetDirectory = result.Directory;
                tokenMapping = result.TokenMapping;

                // Copy any collected native symbols over to the image.
                foreach (var import in symbolProvider.GetImportedModules())
                    image.Imports.Add(import);

                // Copy any collected base relocations over to the image.
                foreach (var relocation in symbolProvider.GetBaseRelocations())
                    image.Relocations.Add(relocation);

                // Copy over debug data.
                for (int i = 0; i < module.DebugData.Count; i++)
                    image.DebugData.Add(module.DebugData[i]);
            }
            catch (Exception ex)
            {
                context.DiagnosticBag.Exceptions.Add(ex);
                context.DiagnosticBag.MarkAsFatal();
            }

            tokenMapping ??= new TokenMapping();
            return new PEImageBuildResult(image, context.DiagnosticBag, tokenMapping);
        }
    }
}
