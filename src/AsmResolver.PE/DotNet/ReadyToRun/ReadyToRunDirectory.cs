using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using AsmResolver.IO;
using AsmResolver.PE.File;

namespace AsmResolver.PE.DotNet.ReadyToRun
{
    /// <summary>
    /// Represents a managed native header of a .NET portable executable file that is in the ReadyToRun format.
    /// </summary>
    public class ReadyToRunDirectory : SegmentBase, IManagedNativeHeader
    {
        private IList<IReadyToRunSection>? _sections;

        /// <inheritdoc />
        public ManagedNativeHeaderSignature Signature => ManagedNativeHeaderSignature.Rtr;

        /// <summary>
        /// Gets the major version of the file format that is used.
        /// </summary>
        public ushort MajorVersion
        {
            get;
            set;
        } = 5;

        /// <summary>
        /// Gets the minor version of the file format that is used.
        /// </summary>
        public ushort MinorVersion
        {
            get;
            set;
        } = 4;

        /// <summary>
        /// Gets or sets the flags associated with the ReadyToRun module.
        /// </summary>
        public ReadyToRunAttributes Attributes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the individual sections referenced by the ReadyToRun directory.
        /// </summary>
        public IList<IReadyToRunSection> Sections
        {
            get
            {
                if (_sections is null)
                    Interlocked.CompareExchange(ref _sections, GetSections(), null);
                return _sections;
            }
        }

        /// <summary>
        /// Obtains the sections referenced by the directory.
        /// </summary>
        /// <returns>The sections.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Sections"/> property.
        /// </remarks>
        protected virtual IList<IReadyToRunSection> GetSections() => new List<IReadyToRunSection>();

        /// <summary>
        /// Gets a section by its section type.
        /// </summary>
        /// <param name="type">The type of section.</param>
        /// <returns>The section.</returns>
        /// <exception cref="ArgumentException">
        /// Occurs when there is no section of the provided type present in the directory.
        /// </exception>
        public IReadyToRunSection GetSection(ReadyToRunSectionType type)
        {
            return TryGetSection(type, out var section)
                ? section
                : throw new ArgumentException($"Directory does not contain a section of type {type}.");
        }

        /// <summary>
        /// Gets a section by its section type.
        /// </summary>
        /// <typeparam name="TSection">The type of section.</typeparam>
        /// <returns>The section.</returns>
        /// <exception cref="ArgumentException">
        /// Occurs when there is no section of the provided type present in the directory.
        /// </exception>
        public TSection GetSection<TSection>()
            where TSection : class, IReadyToRunSection
        {
            return TryGetSection<TSection>(out var section)
                ? section
                : throw new ArgumentException($"Directory does not contain a section of type {typeof(TSection).Name}.");
        }

        /// <summary>
        /// Attempts to get a section by its section type.
        /// </summary>
        /// <param name="type">The type of the section.</param>
        /// <param name="section">The section, or <c>null</c> if none was found.</param>
        /// <returns><c>true</c> if the section was found, <c>false</c> otherwise.</returns>
        public bool TryGetSection(ReadyToRunSectionType type, [NotNullWhen(true)] out IReadyToRunSection? section)
        {
            for (int i = 0; i < Sections.Count; i++)
            {
                if (Sections[i].Type == type)
                {
                    section = Sections[i];
                    return true;
                }
            }

            section = null;
            return false;
        }

        /// <summary>
        /// Attempts to get a section by its section type.
        /// </summary>
        /// <typeparam name="TSection">The type of section.</typeparam>
        /// <param name="section">The section, or <c>null</c> if none was found.</param>
        /// <returns><c>true</c> if the section was found, <c>false</c> otherwise.</returns>
        public bool TryGetSection<TSection>([NotNullWhen(true)] out TSection? section)
            where TSection : class, IReadyToRunSection
        {
            for (int i = 0; i < Sections.Count; i++)
            {
                if (Sections[i] is TSection s)
                {
                    section = s;
                    return true;
                }
            }

            section = null;
            return false;
        }

        /// <inheritdoc />
        public override uint GetPhysicalSize()
        {
            return sizeof(ManagedNativeHeaderSignature) // Signature
                   + sizeof(ushort) // MajorVersion
                   + sizeof(ushort) // MinorVersion
                   + sizeof(ReadyToRunAttributes) // Flags
                   + sizeof(uint) // NumberOfSections
                   + (uint) Sections.Count * (sizeof(ReadyToRunSectionType) + DataDirectory.DataDirectorySize) //Sections
                ;
        }

        /// <inheritdoc />
        public override void Write(IBinaryStreamWriter writer)
        {
            writer.WriteUInt32((uint) Signature);
            writer.WriteUInt16(MajorVersion);
            writer.WriteUInt16(MinorVersion);
            writer.WriteUInt32((uint) Attributes);
            writer.WriteInt32(Sections.Count);

            foreach (var section in Sections)
            {
                writer.WriteUInt32((uint) section.Type);
                writer.WriteUInt32(section.Rva);
                writer.WriteUInt32(section.GetPhysicalSize());
            }
        }
    }
}
