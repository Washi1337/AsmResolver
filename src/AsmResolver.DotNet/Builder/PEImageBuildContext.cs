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
            DiagnosticBag = new DiagnosticBag();
        }
        
        /// <summary>
        /// Creates a new build context.
        /// </summary>
        /// <param name="diagnosticBag">The diagnostic bag to use.</param>
        public PEImageBuildContext(DiagnosticBag diagnosticBag)
        {
            DiagnosticBag = diagnosticBag ?? throw new ArgumentNullException(nameof(diagnosticBag));
        }
        
        /// <summary>
        /// Gets the bag that collects all diagnostic information during the building process. 
        /// </summary>
        public DiagnosticBag DiagnosticBag
        {
            get;
        } 
    }
}