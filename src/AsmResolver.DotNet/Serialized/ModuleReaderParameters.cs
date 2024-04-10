using AsmResolver.IO;
using AsmResolver.PE;
using AsmResolver.PE.DotNet.Metadata;

namespace AsmResolver.DotNet.Serialized
{
    /// <summary>
    /// Provides parameters for the reading process of a .NET module.
    /// </summary>
    public class ModuleReaderParameters
    {
        /// <summary>
        /// Initializes the default module read parameters.
        /// </summary>
        public ModuleReaderParameters()
        {
            MethodBodyReader = DefaultMethodBodyReader.Instance;
            FieldRvaDataReader = AsmResolver.PE.DotNet.Metadata.FieldRvaDataReader.Instance;
            PEReaderParameters = new PEReaderParameters();
        }

        /// <summary>
        /// Initializes the module read parameters with a file service.
        /// </summary>
        /// <param name="context">The context the module should be read in.</param>
        public ModuleReaderParameters(RuntimeContext context)
            : this(context.DefaultReaderParameters)
        {
            RuntimeContext = context;
        }

        /// <summary>
        /// Initializes the module read parameters with a file service.
        /// </summary>
        /// <param name="fileService">The file service to use when reading the file and dependencies.</param>
        public ModuleReaderParameters(IFileService fileService)
            : this(null, new PEReaderParameters {FileService = fileService})
        {
        }

        /// <summary>
        /// Initializes the module read parameters with an error listener.
        /// </summary>
        /// <param name="errorListener">The object responsible for recording parser errors.</param>
        public ModuleReaderParameters(IErrorListener errorListener)
            : this(null, errorListener)
        {
        }

        /// <summary>
        /// Initializes the module read parameters with a working directory.
        /// </summary>
        /// <param name="workingDirectory">The working directory of the modules to read.</param>
        public ModuleReaderParameters(string? workingDirectory)
            : this(workingDirectory, ThrowErrorListener.Instance)
        {
        }

        /// <summary>
        /// Initializes the module read parameters with a working directory.
        /// </summary>
        /// <param name="workingDirectory">The working directory of the modules to read.</param>
        /// <param name="errorListener">The object responsible for recording parser errors.</param>
        public ModuleReaderParameters(string? workingDirectory, IErrorListener errorListener)
            : this(workingDirectory, new PEReaderParameters(errorListener))
        {
        }

        /// <summary>
        /// Initializes the module read parameters with a working directory.
        /// </summary>
        /// <param name="workingDirectory">The working directory of the modules to read.</param>
        /// <param name="readerParameters">The parameters to use while reading the assembly and its dependencies.</param>
        public ModuleReaderParameters(string? workingDirectory, PEReaderParameters readerParameters)
        {
            if (workingDirectory is not null)
            {
                WorkingDirectory = workingDirectory;
                ModuleResolver = new DirectoryNetModuleResolver(workingDirectory, this);
            }

            MethodBodyReader = DefaultMethodBodyReader.Instance;
            FieldRvaDataReader = AsmResolver.PE.DotNet.Metadata.FieldRvaDataReader.Instance;
            PEReaderParameters = readerParameters;
        }

        /// <summary>
        /// Clones the provided module reader parameters.
        /// </summary>
        /// <param name="readerParameters">The parameters to clone.</param>
        public ModuleReaderParameters(ModuleReaderParameters readerParameters)
        {
            WorkingDirectory = readerParameters.WorkingDirectory;
            ModuleResolver = readerParameters.ModuleResolver;
            MethodBodyReader = readerParameters.MethodBodyReader;
            FieldRvaDataReader = readerParameters.FieldRvaDataReader;
            PEReaderParameters = readerParameters.PEReaderParameters;
            RuntimeContext = readerParameters.RuntimeContext;
        }

        /// <summary>
        /// Gets the working directory of the module to read.
        /// </summary>
        public string? WorkingDirectory
        {
            get;
        }

        /// <summary>
        /// Gets or sets the object used for resolving a net module.
        /// </summary>
        public INetModuleResolver? ModuleResolver
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the method body parser.
        /// </summary>
        public IMethodBodyReader MethodBodyReader
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the field initial value reader.
        /// </summary>
        public IFieldRvaDataReader FieldRvaDataReader
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the parameters used for parsing a PE file into a PE image.
        /// </summary>
        /// <remarks>
        /// This property is ignored when the module was read from a <see cref="IPEImage"/>
        /// </remarks>
        public PEReaderParameters PEReaderParameters
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the runtime context to load the module in, or <c>null</c> if a new context is to be created.
        /// </summary>
        public RuntimeContext? RuntimeContext
        {
            get;
            set;
        }
    }
}
