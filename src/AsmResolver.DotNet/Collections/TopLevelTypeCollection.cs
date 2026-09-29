using System.Collections.Generic;
using System.Threading;

namespace AsmResolver.DotNet.Collections
{
    /// <summary>
    /// Stores a module's top-level types and supports repeated lookups by namespace and name.
    /// </summary>
    internal sealed class TopLevelTypeCollection : MemberCollection<ITypeOwner, TypeDefinition>
    {
        // Type-name parsing creates fresh references, so the runtime context's identity cache
        // cannot avoid repeated scans. Keep this index local to the module that owns the types.
        // Repeated indexed lookups become faster around 32 types in small-module probes;
        // below that, scanning avoids the dictionary's up-front allocation.
        private const int IndexThreshold = 32;
        private int _version;
        private Index? _index;

        /// <summary>
        /// Creates an empty collection owned by the specified module.
        /// </summary>
        /// <param name="module">The module defining the types.</param>
        internal TopLevelTypeCollection(ModuleDefinition module)
            : base(module)
        {
        }

        /// <summary>
        /// Creates an empty collection with the specified initial capacity.
        /// </summary>
        /// <param name="module">The module defining the types.</param>
        /// <param name="capacity">The initial capacity.</param>
        internal TopLevelTypeCollection(ModuleDefinition module, int capacity)
            : base(module, capacity)
        {
        }

        /// <summary>
        /// Finds the first top-level type matching a namespace and name.
        /// </summary>
        /// <param name="ns">The namespace to match.</param>
        /// <param name="name">The name to match.</param>
        /// <param name="definition">The matching type, or <c>null</c> if none exists.</param>
        /// <returns><c>true</c> if a matching type was found; otherwise, <c>false</c>.</returns>
        internal bool TryFind(Utf8String? ns, Utf8String name, out TypeDefinition? definition)
        {
            if (Count < IndexThreshold)
            {
                for (int i = 0; i < Count; i++)
                {
                    var candidate = this[i];

                    if (candidate.IsTypeOfUtf8(ns, name))
                    {
                        definition = candidate;
                        return true;
                    }
                }

                definition = null;
                return false;
            }

            // Published snapshots are read without locking; the version rejects snapshots invalidated by a setter.
            var index = Interlocked.CompareExchange(ref _index, null, null);

            if (index is null || index.Version != Interlocked.CompareExchange(ref _version, 0, 0))
            {
                // LazyList mutations lock Items, so construction cannot interleave with collection edits.
                lock (Items)
                {
                    index = Interlocked.CompareExchange(ref _index, null, null);

                    while (index is null || index.Version != Interlocked.CompareExchange(ref _version, 0, 0))
                    {
                        int version = Interlocked.CompareExchange(ref _version, 0, 0);
                        var definitions = new Dictionary<(Utf8String?, Utf8String), TypeDefinition>(Items.Count);

                        foreach (var candidate in Items)
                        {
                            if (candidate.Name is { } candidateName)
                            {
                                var key = (candidate.Namespace, candidateName);

                                // Preserve the first definition in collection order when names are duplicated.
#if NETCOREAPP || NETSTANDARD2_1_OR_GREATER
                                definitions.TryAdd(key, candidate);
#else
                                if (!definitions.ContainsKey(key))
                                    definitions.Add(key, candidate);
#endif
                            }
                        }

                        // Name setters invalidate without taking Items; retry if one ran during construction.
                        if (version == Interlocked.CompareExchange(ref _version, 0, 0))
                        {
                            index = new Index(version, definitions);
                            Interlocked.Exchange(ref _index, index);
                        }
                    }
                }
            }

            return index.Definitions.TryGetValue((ns, name), out definition);
        }

        /// <summary>
        /// Discards the indexed snapshot after a collection or top-level type name change.
        /// </summary>
        internal void Invalidate()
        {
            // Bump first so a builder racing with this setter cannot publish a stale snapshot as current.
            Interlocked.Increment(ref _version);
            Interlocked.Exchange(ref _index, null);
        }

        /// <inheritdoc />
        protected override void OnSetItem(int index, TypeDefinition item)
        {
            // Base hooks may change ownership before throwing; invalidate even on partial mutations.
            try
            {
                base.OnSetItem(index, item);
            }
            finally
            {
                Invalidate();
            }
        }

        /// <inheritdoc />
        protected override void OnInsertItem(int index, TypeDefinition item)
        {
            try
            {
                base.OnInsertItem(index, item);
            }
            finally
            {
                Invalidate();
            }
        }

        /// <inheritdoc />
        protected override void OnInsertRange(int index, IEnumerable<TypeDefinition> items)
        {
            try
            {
                base.OnInsertRange(index, items);
            }
            finally
            {
                Invalidate();
            }
        }

        /// <inheritdoc />
        protected override void OnRemoveItem(int index)
        {
            try
            {
                base.OnRemoveItem(index);
            }
            finally
            {
                Invalidate();
            }
        }

        /// <inheritdoc />
        protected override void OnClearItems()
        {
            try
            {
                base.OnClearItems();
            }
            finally
            {
                Invalidate();
            }
        }

        /// <summary>
        /// An immutable lookup snapshot for one collection version.
        /// </summary>
        /// <param name="version">The collection version used to build the snapshot.</param>
        /// <param name="definitions">The first definition for each namespace and name.</param>
        private sealed class Index(int version, Dictionary<(Utf8String?, Utf8String), TypeDefinition> definitions)
        {
            /// <summary>Gets the collection version represented by the snapshot.</summary>
            public int Version { get; } = version;

            /// <summary>Gets the first matching definition for each namespace and name.</summary>
            public Dictionary<(Utf8String?, Utf8String), TypeDefinition> Definitions { get; } = definitions;
        }
    }
}
