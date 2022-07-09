using System;
using System.Diagnostics.CodeAnalysis;
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
        /// <param name="tokenMapping">An object that maps metadata members to their newly assigned tokens.</param>
        public PEImageBuildResult(IPEImage? image, DiagnosticBag diagnosticBag, ITokenMapping tokenMapping)
        {
            ConstructedImage = image;
            DiagnosticBag = diagnosticBag ?? throw new ArgumentNullException(nameof(diagnosticBag));
            TokenMapping = tokenMapping ?? throw new ArgumentNullException(nameof(tokenMapping));
        }

        /// <summary>
        /// Gets the constructed image, or <c>null</c> if the construction failed.
        /// </summary>
        public IPEImage? ConstructedImage
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether the image was constructed successfully or not.
        /// </summary>
        [MemberNotNullWhen(false, nameof(ConstructedImage))]
        public bool HasFailed => DiagnosticBag.IsFatal;

        /// <summary>
        /// Gets the bag containing the diagnostics that were collected during the construction of the image.
        /// </summary>
        public DiagnosticBag DiagnosticBag
        {
            get;
        }

        /// <summary>
        /// Gets an object that maps metadata members to their newly assigned tokens.
        /// </summary>
        public ITokenMapping TokenMapping
        {
            get;
        }
    }
}
