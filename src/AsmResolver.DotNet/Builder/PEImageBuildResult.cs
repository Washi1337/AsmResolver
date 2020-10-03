using AsmResolver.PE;

namespace AsmResolver.DotNet.Builder
{
    /// <summary>
    /// Describes the result of the construction of a <see cref="IPEImage"/> from a <see cref="ModuleDefinition"/>.
    /// </summary>
    public class PEImageBuildResult
    {
        /// <summary>
        /// Creates a new instance of the <see cref="PEImageBuildResult"/> class. 
        /// </summary>
        /// <param name="image">The constructed image, or <c>null</c> if the construction failed.</param>
        /// <param name="diagnosticBag">The diagnostics that were collected during the construction of the image.</param>
        public PEImageBuildResult(IPEImage image, DiagnosticBag diagnosticBag)
        {
            ConstructedImage = image;
            DiagnosticBag = diagnosticBag;
        }

        /// <summary>
        /// Gets the constructed image, or <c>null</c> if the construction failed.
        /// </summary>
        public IPEImage ConstructedImage
        {
            get;
        }

        /// <summary>
        /// Gets the bag containing the diagnostics that were collected during the construction of the image.
        /// </summary>
        public DiagnosticBag DiagnosticBag
        {
            get;
        }
    }
}