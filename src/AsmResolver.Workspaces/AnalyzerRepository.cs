using System;
using System.Collections.Generic;
using System.Linq;

namespace AsmResolver.Workspaces
{
    /// <summary>
    /// Implements a collection of analyzers that are sorted by subject type.
    /// </summary>
    public class AnalyzerRepository
    {
        private readonly Dictionary<Type, Node> _analyzers = new();

        /// <summary>
        /// Registers a new object analyser for the provided object type.
        /// </summary>
        /// <param name="subjectType">The type of objects to analyze.</param>
        /// <param name="analyzer">The object analyzer.</param>
        public void Register(Type subjectType, IObjectAnalyzer analyzer)
        {
            if (!_analyzers.TryGetValue(subjectType, out var node))
            {
                node = new Node(subjectType);
                _analyzers.Add(subjectType, node);
            }

            node.Analyzers.Add(analyzer);
        }

        /// <summary>
        /// Gets a collection of analyzers that are able to analyze objects of the privded type.
        /// </summary>
        /// <param name="type">The type of objects to analyze.</param>
        /// <returns>The collection of analyzers.</returns>
        public IEnumerable<IObjectAnalyzer> GetAnalyzers(Type? type)
        {
            while (type is not null)
            {
                if (_analyzers.TryGetValue(type, out var node))
                {
                    foreach (var analyzer in node.Analyzers)
                        yield return analyzer;
                }

                type = type.BaseType;
            }
        }

        private sealed class Node
        {
            public Node(Type type)
            {
                Type = type;
            }

            public Type Type
            {
                get;
            }

            public ICollection<IObjectAnalyzer> Analyzers
            {
                get;
            } = new HashSet<IObjectAnalyzer>();
        }
    }
}
