using System;

namespace AsmResolver.DotNet.Builder
{
    /// <summary>
    /// Provides a context in which a PE image construction takes place in.
    /// </summary>
    public class PEImageBuildContext
    {
        /// <summary>
        /// Creates a new empty build context.
        /// </summary>
        public PEImageBuildContext()
        {
            ErrorListener = new DiagnosticBag();
        }

        /// <summary>
        /// Creates a new build context.
        /// </summary>
        /// <param name="diagnosticBag">The diagnostic bag to use.</param>
        public PEImageBuildContext(DiagnosticBag diagnosticBag)
        {
            ErrorListener = diagnosticBag ?? throw new ArgumentNullException(nameof(diagnosticBag));
        }

        /// <summary>
        /// Creates a new build context.
        /// </summary>
        /// <param name="errorListener">The diagnostic bag to use.</param>
        public PEImageBuildContext(IErrorListener errorListener)
        {
            ErrorListener = errorListener ?? throw new ArgumentNullException(nameof(errorListener));
        }

        /// <summary>
        /// Gets the bag that collects all diagnostic information during the building process.
        /// </summary>
        [Obsolete("Use ErrorListener instead.")]
        public DiagnosticBag? DiagnosticBag => ErrorListener as DiagnosticBag;

        /// <summary>
        /// Gets the error listener that handles all diagnostic information during the building process.
        /// </summary>
        public IErrorListener ErrorListener
        {
            get;
        }
    }
}
