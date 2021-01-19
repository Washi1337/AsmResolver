using System;
using System.Collections.Generic;
using AsmResolver.PE;
using AsmResolver.PE.DotNet.Metadata.Tables;

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
        /// <param name="tokenMapping">A dictionary that maps metadata members to their newly assigned tokens.</param>
        public PEImageBuildResult(IPEImage image, DiagnosticBag diagnosticBag, IReadOnlyDictionary<IMetadataMember, MetadataToken> tokenMapping)
        {
            ConstructedImage = image;
            DiagnosticBag = diagnosticBag ?? throw new ArgumentNullException(nameof(diagnosticBag));
            TokenMapping = tokenMapping ?? throw new ArgumentNullException(nameof(tokenMapping));
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

        /// <summary>
        /// Gets a dictionary that maps metadata members to their newly assigned tokens.
        /// </summary>
        public IReadOnlyDictionary<IMetadataMember, MetadataToken> TokenMapping
        {
            get;
        }
    }
}