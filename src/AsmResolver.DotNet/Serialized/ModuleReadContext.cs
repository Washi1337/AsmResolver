using System;
using AsmResolver.PE;

namespace AsmResolver.DotNet.Serialized
{
    public class ModuleReadContext : IErrorListener
    {
        public ModuleReadContext(IPEImage image, SerializedModuleDefinition parentModule, ModuleReadParameters parameters)
        {
            Image = image ?? throw new ArgumentNullException(nameof(image));
            ParentModule = parentModule ?? throw new ArgumentNullException(nameof(parentModule));
            Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        }

        public IPEImage Image
        {
            get;
        }
        
        public SerializedModuleDefinition ParentModule
        {
            get;
        }
    
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