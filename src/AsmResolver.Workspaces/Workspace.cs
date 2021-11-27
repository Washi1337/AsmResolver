﻿using System.Collections.Generic;

namespace AsmResolver.Workspaces
{
    /// <summary>
    /// Provides a base mechanism for indexing assemblies and their components.
    /// </summary>
    public abstract class Workspace
    {
        /// <summary>
        /// Gets a ordered list of profiles for workspace analyzing.
        /// </summary>
        public List<WorkspaceProfile> Profiles
        {
            get;
        } = new();

        /// <summary>
        /// Gets the index containing all analyzed objects.
        /// </summary>
        public WorkspaceIndex Index
        {
            get;
        } = new();

        /// <summary>
        /// Performs the analysis.
        /// </summary>
        /// <param name="context">The analysis context.</param>
        protected void Analyze(AnalysisContext context)
        {
            var traversedObjects = new HashSet<object>(context.Agenda);
            foreach (var profile in Profiles)
            {
                context.Agenda.Clear();
                foreach (var agenda in traversedObjects)
                    context.Agenda.Enqueue(agenda);
                context.TraversedObjects.Clear();

                while (context.Agenda.Count > 0)
                {
                    var nextSubject = context.Agenda.Dequeue();
                    var analyzers = profile.Analyzers.GetAnalyzers(nextSubject.GetType());
                    foreach (var analyzer in analyzers)
                    {
                        if (analyzer.CanAnalyze(context, nextSubject))
                            analyzer.Analyze(context, nextSubject);
                    }
                }

                foreach (var traversedObject in context.TraversedObjects)
                    traversedObjects.Add(traversedObject);
            }
            
            foreach (var traversedObject in traversedObjects)
                context.TraversedObjects.Add(traversedObject);
        }
    }
}
