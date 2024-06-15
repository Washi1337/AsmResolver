using System;
using System.Diagnostics.CodeAnalysis;
using AsmResolver.PE;

namespace AsmResolver.DotNet.Builder
{
    /// <summary>
    /// Describes the result of the construction of a <see cref="PEImage"/> from a <see cref="ModuleDefinition"/>.
    /// </summary>
    public class PEImageBuildResult
    {
        /// <summary>
        /// Creates a new instance of the <see cref="PEImageBuildResult"/> class.
        /// </summary>
        /// <param name="image">The constructed image, or <c>null</c> if the construction failed.</param>
        /// <param name="diagnosticBag">The diagnostics that were collected during the construction of the image.</param>
        /// <param name="tokenMapping">An object that maps metadata members to their newly assigned tokens.</param>
        [Obsolete]
        public PEImageBuildResult(PEImage? image, DiagnosticBag diagnosticBag, ITokenMapping tokenMapping)
        {
            ConstructedImage = image;
            ErrorListener = diagnosticBag ?? throw new ArgumentNullException(nameof(diagnosticBag));
            TokenMapping = tokenMapping ?? throw new ArgumentNullException(nameof(tokenMapping));
        }

        /// <summary>
        /// Creates a new instance of the <see cref="PEImageBuildResult"/> class.
        /// </summary>
        /// <param name="image">The constructed image, or <c>null</c> if the construction failed.</param>
        /// <param name="errorListener">The diagnostics that were collected during the construction of the image.</param>
        /// <param name="tokenMapping">An object that maps metadata members to their newly assigned tokens.</param>
        public PEImageBuildResult(PEImage? image, IErrorListener errorListener, ITokenMapping tokenMapping)
        {
            ConstructedImage = image;
            ErrorListener = errorListener ?? throw new ArgumentNullException(nameof(errorListener));
            TokenMapping = tokenMapping ?? throw new ArgumentNullException(nameof(tokenMapping));
        }

        /// <summary>
        /// Gets the constructed image, or <c>null</c> if the construction failed.
        /// </summary>
        public PEImage? ConstructedImage
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether the image was constructed successfully or not.
        /// </summary>
        [MemberNotNullWhen(false, nameof(ConstructedImage))]
        public bool HasFailed => ConstructedImage is null;

        /// <summary>
        /// Gets the bag containing the diagnostics that were collected during the construction of the image (if available).
        /// </summary>
        [Obsolete("Use the ErrorListener property instead.")]
        public DiagnosticBag? DiagnosticBag => ErrorListener as DiagnosticBag;

        /// <summary>
        /// Gets the error listener handling the diagnostics that were collected during the construction of the image.
        /// </summary>
        public IErrorListener ErrorListener
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
