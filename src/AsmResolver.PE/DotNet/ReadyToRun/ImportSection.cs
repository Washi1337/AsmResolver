using System.Diagnostics;
using System.Threading;
using AsmResolver.Collections;
using AsmResolver.IO;
using AsmResolver.PE.File;

namespace AsmResolver.PE.DotNet.ReadyToRun
{
    /// <summary>
    /// Represents a single import section in a ReadyToRun ImportSections section.
    /// </summary>
    [DebuggerDisplay("{Type}, Slots = {Slots.Count} ({Attributes})")]
    public class ImportSection : IWritable
    {
        internal const uint ImportSectionSize =
                DataDirectory.DataDirectorySize // Section
                + sizeof(ImportSectionAttributes) // Flags
                + sizeof(ImportSectionType) // Type
                + sizeof(uint) // Signatures
                + sizeof(uint) // AuxiliaryData
            ;

        private ReferenceTable? _slots;
        private ReferenceTable? _signatures;

        /// <summary>
        /// Gets a collection of slots stored in the import section.
        /// </summary>
        public ReferenceTable Slots
        {
            get
            {
                if (_slots is null)
                    Interlocked.CompareExchange(ref _slots, GetSlots(), null);
                return _slots;
            }
        }

        /// <summary>
        /// Gets or sets attributes associated to the section.
        /// </summary>
        public ImportSectionAttributes Attributes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the type of slots stored in <see cref="Slots"/>.
        /// </summary>
        public ImportSectionType Type
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the size in bytes of a single slot in <see cref="Slots"/>.
        /// </summary>
        /// <remarks>
        /// Valid values are either 4 or 8.
        /// </remarks>
        public byte EntrySize
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a collection of signatures associated with each slot in <see cref="Slots"/> stored in the import
        /// section (if available).
        /// </summary>
        public ReferenceTable Signatures
        {
            get
            {
                if (_signatures is null)
                    Interlocked.CompareExchange(ref _signatures, GetSignatures(), null);
                return _signatures;
            }
        }

        /// <summary>
        /// Gets a pointer to auxiliary data attached to this section, if available.
        /// </summary>
        public ISegmentReference AuxiliaryData
        {
            get;
            set;
        } = SegmentReference.Null;

        /// <summary>
        /// Obtains the slots stored in this section.
        /// </summary>
        /// <returns>The slots.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Slots"/> property.
        /// </remarks>
        protected virtual ReferenceTable GetSlots() => new(ReferenceTableAttributes.Va | ReferenceTableAttributes.Adaptive);

        /// <summary>
        /// Obtains the signatures stored in this section.
        /// </summary>
        /// <returns>The signatures.</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="Signatures"/> property.
        /// </remarks>
        protected virtual ReferenceTable GetSignatures() => new(ReferenceTableAttributes.Rva | ReferenceTableAttributes.Force32Bit);

        /// <inheritdoc />
        public uint GetPhysicalSize() => ImportSectionSize;

        /// <inheritdoc />
        public void Write(BinaryStreamWriter writer)
        {
            writer.WriteUInt32(Slots.Rva);
            writer.WriteUInt32(Slots.GetPhysicalSize());
            writer.WriteUInt16((ushort) Attributes);
            writer.WriteByte((byte) Type);
            writer.WriteByte(EntrySize);
            writer.WriteUInt32(Signatures.Count == 0 ? 0 : Signatures.Rva);
            writer.WriteUInt32(AuxiliaryData.Rva);
        }
    }
}
