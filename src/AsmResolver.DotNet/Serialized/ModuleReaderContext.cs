using System;
using AsmResolver.PE;
using AsmResolver.PE.DotNet.Metadata;

namespace AsmResolver.DotNet.Serialized
{
    /// <summary>
    /// Provides a context in which a .NET module parser exists in. This includes the original PE image, as well as the
    /// module reader parameters.
    /// </summary>
    public class ModuleReaderContext : IErrorListener
    {
        /// <summary>
        /// Creates a new instance of the <see cref="ModuleReaderContext"/> class.
        /// </summary>
        /// <param name="image">The original PE image to read from.</param>
        /// <param name="parentModule">The root module object.</param>
        /// <param name="parameters">The module reader parameters.</param>
        public ModuleReaderContext(IPEImage image, SerializedModuleDefinition parentModule, ModuleReaderParameters parameters)
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
        /// Gets the original metadata directory.
        /// </summary>
        public IMetadata Metadata => Image.DotNetDirectory!.Metadata!;

        /// <summary>
        /// Gets the reader parameters.
        /// </summary>
        public ModuleReaderParameters Parameters
        {
            get;
        }

        /// <inheritdoc />
        public void MarkAsFatal() => Parameters.PEReaderParameters.ErrorListener.MarkAsFatal();

        /// <inheritdoc />
        public void RegisterException(Exception exception) =>
            Parameters.PEReaderParameters.ErrorListener.RegisterException(exception);
    }
}
