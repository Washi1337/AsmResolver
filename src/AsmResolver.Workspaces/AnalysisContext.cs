using System;
using System.Collections.Generic;
using System.Linq;

namespace AsmResolver.Workspaces
{
    /// <summary>
    /// Provides a context in which a workspace analyzer is situated in.
    /// </summary>
    public class AnalysisContext
    {
        /// <summary>
        /// Creates a new instance of the <see cref="AnalysisContext"/> class.
        /// </summary>
        /// <param name="workspace">The parent workspace.</param>
        public AnalysisContext(Workspace workspace)
        {
            Workspace = workspace;
        }

        /// <summary>
        /// Gets the parent workspace that this analysis context is associated with.
        /// </summary>
        public Workspace Workspace
        {
            get;
        }

        /// <summary>
        /// Gets a queue of objects that are scheduled for analysis.
        /// </summary>
        public Queue<object> Agenda
        {
            get;
        } = new();

        /// <summary>
        /// Gets a collection of objects that were already analysed.
        /// </summary>
        public ISet<object> TraversedObjects
        {
            get;
        } = new HashSet<object>();

        /// <summary>
        /// Determines whether the provided object type can be analyzed by at least one analyzer in this repository.
        /// </summary>
        /// <param name="type">The type of objects to analyze.</param>
        /// <returns>
        /// <c>true</c> if there exists at least one analyzer that can analyze objects of the provided type,
        /// <c>false</c> otherwise.
        /// </returns>
        public bool HasAnalyzers(Type type) => Workspace.Profiles.Any(profile => profile.Analyzers.HasAnalyzers(type));

        /// <summary>
        /// Schedules the provided object if it was not scheduled before.
        /// </summary>
        /// <param name="subject">The object to analyse.</param>
        public void ScheduleForAnalysis(object subject)
        {
            if (TraversedObjects.Add(subject))
                Agenda.Enqueue(subject);
        }
    }
}
