using System;
using AsmResolver.PE;

namespace AsmResolver.DotNet.Serialized
{
    /// <summary>
    /// Provides a context in which a .NET module parser exists in. This includes the original PE image, as well as the
    /// module reader parameters. 
    /// </summary>
    public class ModuleReadContext : IErrorListener
    {
        /// <summary>
        /// Creates a new instance of the <see cref="ModuleReadContext"/> class.
        /// </summary>
        /// <param name="image">The original PE image to read from.</param>
        /// <param name="parentModule">The root module object.</param>
        /// <param name="parameters">The module reader parameters.</param>
        public ModuleReadContext(IPEImage image, SerializedModuleDefinition parentModule, ModuleReadParameters parameters)
        {
            Image = image ?? throw new ArgumentNullException(nameof(image));
            ParentModule = parentModule ?? throw new ArgumentNullException(nameof(parentModule));
            Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        }

        /// <summary>
        /// Gets the original PE image to read from.
        /// </summary>
        public IPEImage Image
        {
            get;
        }
        
        /// <summary>
        /// Gets the root module object that is being read.
        /// </summary>
        public SerializedModuleDefinition ParentModule
        {
            get;
        }
    
        /// <summary>
        /// Gets the reader parameters.
        /// </summary>
        public ModuleReadParameters Parameters
        {
            get;
        }

        /// <inheritdoc />
        public void MarkAsFatal() => Parameters.PEReadParameters.ErrorListener.MarkAsFatal();

        /// <inheritdoc />
        public void RegisterException(Exception exception) =>
            Parameters.PEReadParameters.ErrorListener.RegisterException(exception);
    }
}