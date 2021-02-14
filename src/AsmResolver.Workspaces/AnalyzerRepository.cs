using System;
using System.Collections.Generic;

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
        /// Determines whether the provided object type can be analyzed by at least one analyzer in this repository.
        /// </summary>
        /// <param name="type">The type of objects to analyze.</param>
        /// <returns>
        /// <c>true</c> if there exists at least one analyzer that can analyze objects of the provided type,
        /// <c>false</c> otherwise.
        /// </returns>
        public bool HasAnalyzers(Type? type)
        {
            while (type is not null)
            {
                if (_analyzers.TryGetValue(type, out var node))
                {
                    if (node.Analyzers.Count > 0)
                        return true;
                }

                type = type.BaseType;
            }

            return false;
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

            public HashSet<IObjectAnalyzer> Analyzers
            {
                get;
            } = new();
        }
    }
}
