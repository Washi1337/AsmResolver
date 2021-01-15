using System;
using AsmResolver.PE;
using AsmResolver.PE.DotNet.Metadata;

namespace AsmResolver.DotNet.Serialized
{
    /// <summary>
    /// Provides parameters for the reading process of a .NET module.
    /// </summary>
    public class ModuleReadParameters
    {
        /// <summary>
        /// Initializes the default module read parameters.
        /// </summary>
        public ModuleReadParameters()
        {
        }

        /// <summary>
        /// Initializes the module read parameters with an error listener.
        /// </summary>
        /// <param name="errorListener">The object responsible for recording parser errors.</param>
        public ModuleReadParameters(IErrorListener errorListener)
            : this(null,errorListener)
        {
        }

        /// <summary>
        /// Initializes the module read parameters with a working directory.
        /// </summary>
        /// <param name="workingDirectory">The working directory of the modules to read.</param>
        public ModuleReadParameters(string workingDirectory)
            : this(workingDirectory, ThrowErrorListener.Instance)
        {
        }

        /// <summary>
        /// Initializes the module read parameters with a working directory.
        /// </summary>
        /// <param name="workingDirectory">The working directory of the modules to read.</param>
        /// <param name="errorListener">The object responsible for recording parser errors.</param>
        public ModuleReadParameters(string workingDirectory, IErrorListener errorListener)
        {
            if (workingDirectory != null)
            {
                WorkingDirectory = workingDirectory;
                ModuleResolver = new DirectoryNetModuleResolver(workingDirectory, this);
            }

            PEReadParameters.ErrorListener = errorListener;
        }

        /// <summary>
        /// Gets the working directory of the module to read.
        /// </summary>
        public string WorkingDirectory
        {
            get;
        }

        /// <summary>
        /// Gets or sets the object used for resolving a net module.
        /// </summary>
        public INetModuleResolver ModuleResolver
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
        } = new DefaultMethodBodyReader();

        /// <summary>
        /// Gets or sets the field initial value reader.
        /// </summary>
        public IFieldRvaDataReader FieldRvaDataReader
        {
            get;
            set;
        } = new FieldRvaDataReader();

        /// <summary>
        /// Gets or sets the parameters used for parsing a PE file into a PE image.
        /// </summary>
        /// <remarks>
        /// This property is ignored when the module was read from a <see cref="IPEImage"/>
        /// </remarks>
        public PEReadParameters PEReadParameters
        {
            get;
            set;
        } = new PEReadParameters();
    }
}