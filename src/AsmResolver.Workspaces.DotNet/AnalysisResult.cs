using System.Collections.Generic;

namespace AsmResolver.Workspaces.DotNet
{
    /// <summary>
    /// Provides a report describing the results of a workspace analysis process
    /// </summary>
    public class AnalysisResult
    {
        internal AnalysisResult(ISet<object> traversedObjects)
        {
            TraversedObjects = traversedObjects;
        }

        /// <summary>
        /// Gets a collection of objects that were analysed.
        /// </summary>
        public ISet<object> TraversedObjects
        {
            get;
        }
    }
}
