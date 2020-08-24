using System;
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

            PEImage image = null;
            try
            {
                image = new PEImage
                {
                    MachineType = module.MachineType,
                    PEKind = module.PEKind,
                    Characteristics = module.FileCharacteristics,
                    SubSystem = module.SubSystem,
                    DllCharacteristics = module.DllCharacteristics,
                    DotNetDirectory = DotNetDirectoryFactory.CreateDotNetDirectory(module, context.DiagnosticBag),
                    Resources = module.NativeResourceDirectory,
                };
            }
            catch (Exception ex)
            {
                context.DiagnosticBag.Exceptions.Add(ex);
                context.DiagnosticBag.MarkAsFatal();
            }

            return new PEImageBuildResult(image, context.DiagnosticBag);
        }
    }
}