using System;
using System.Collections.Generic;
using System.Threading;
using AsmResolver.IO;

namespace AsmResolver.PE.DotNet.ReadyToRun
{
    /// <summary>
    /// Represents a section in the ReadyToRun directory of a .NET module that contains import sections.
    /// </summary>
    public class ImportSectionsSection : SegmentBase, IReadyToRunSection
    {
        private IList<ImportSection>? _sections;

        /// <inheritdoc />
        public ReadyToRunSectionType Type => ReadyToRunSectionType.ImportSections;

        /// <summary>
        /// Gets the import sub-sections stored in the section.
        /// </summary>
        public IList<ImportSection> Sections
        {
            get
            {
                if (_sections is null)
                    Interlocked.CompareExchange(ref _sections, GetSections(), null);
                return _sections;
            }
        }

        /// <summary>
        /// Obtains the sub sections stored in the section.
        /// </summary>
        /// <returns>The sections.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Sections"/> property.
        /// </remarks>
        protected virtual IList<ImportSection> GetSections() => new List<ImportSection>();

        /// <inheritdoc />
        public virtual bool CanRead => false;

        /// <inheritdoc />
        public virtual BinaryStreamReader CreateReader() => throw new InvalidOperationException();

        /// <inheritdoc />
        public override uint GetPhysicalSize() => (uint) Sections.Count * ImportSection.ImportSectionSize;

        /// <inheritdoc />
        public override void Write(IBinaryStreamWriter writer)
        {
            for (int i = 0; i < Sections.Count; i++)
                Sections[i].Write(writer);
        }
    }
}
