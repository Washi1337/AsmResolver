using System;
using System.Threading;
using AsmResolver.IO;

namespace AsmResolver.PE.DotNet.ReadyToRun
{
    /// <summary>
    /// Represents the ReadyToRun section containing additional debugging information for precompiled methods.
    /// </summary>
    public class DebugInfoSection : SegmentBase, IReadyToRunSection
    {
        private NativeArray<DebugInfo>? _entries;

        /// <inheritdoc />
        public ReadyToRunSectionType Type => ReadyToRunSectionType.DebugInfo;

        /// <inheritdoc />
        public virtual bool CanRead => false;

        /// <summary>
        /// Gets an ordered collection of debug info entries stored in the section.
        /// </summary>
        public NativeArray<DebugInfo> Entries
        {
            get
            {
                if (_entries is null)
                    Interlocked.CompareExchange(ref _entries, GetEntries(), null);
                return _entries;
            }
        }

        /// <summary>
        /// Obtains the entries stored in the section.
        /// </summary>
        /// <returns>The entries.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Entries"/> property.
        /// </remarks>
        protected virtual NativeArray<DebugInfo> GetEntries() => new();

        /// <inheritdoc />
        public virtual BinaryStreamReader CreateReader() => throw new InvalidOperationException();

        /// <inheritdoc />
        public override void UpdateOffsets(in RelocationParameters parameters)
        {
            base.UpdateOffsets(in parameters);
            Entries.UpdateOffsets(in parameters);
        }

        /// <inheritdoc />
        public override uint GetPhysicalSize() => Entries.GetPhysicalSize();

        /// <inheritdoc />
        public override void Write(BinaryStreamWriter writer) => Entries.Write(writer);
    }
}
