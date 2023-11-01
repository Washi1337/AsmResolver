using System.Collections.Generic;
using System.Threading;
using AsmResolver.IO;
using AsmResolver.PE.File.Headers;

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
        public ReadyToRunCoreHeaderAttributes Attributes
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

        /// <inheritdoc />
        public override uint GetPhysicalSize()
        {
            return sizeof(ManagedNativeHeaderSignature) // Signature
                   + sizeof(ushort) // MajorVersion
                   + sizeof(ushort) // MinorVersion
                   + sizeof(ReadyToRunCoreHeaderAttributes) // Flags
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
