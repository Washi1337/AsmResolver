using AsmResolver.DotNet.Code;
using AsmResolver.DotNet.Code.Cil;

namespace AsmResolver.DotNet.Builder
{
    /// <summary>
    /// Defines parameters for constructing a .NET data directory.
    /// </summary>
    public class DotNetDirectoryBuilderParameters 
    {
        /// <summary>
        /// Gets or sets the flags defining the behaviour of the .NET metadata directory builder.
        /// </summary>
        public MetadataBuilderFlags MetadataBuilderFlags
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the method body serializer to use for constructing method bodies.
        /// </summary>
        public IMethodBodySerializer MethodBodySerializer
        {
            get;
            set;
        } = new CilMethodBodySerializer();
    }
}