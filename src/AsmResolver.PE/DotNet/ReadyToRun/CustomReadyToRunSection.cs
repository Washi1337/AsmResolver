using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using AsmResolver.IO;

namespace AsmResolver.PE.DotNet.ReadyToRun
{
    /// <summary>
    /// Represents a ReadyToRun section with a custom or unsupported file format.
    /// </summary>
    [DebuggerDisplay(nameof(CustomReadyToRunSection) + " ({"  + nameof(Type) + "})")]
    public sealed class CustomReadyToRunSection : SegmentBase, IReadyToRunSection
    {
        /// <summary>
        /// Creates a new ReadyToRun section with a custom format.
        /// </summary>
        /// <param name="type">The type of the section.</param>
        /// <param name="contents">The contents of the section.</param>
        public CustomReadyToRunSection(ReadyToRunSectionType type, ISegment contents)
        {
            Type = type;
            Contents = contents;
        }

        /// <inheritdoc />
        public ReadyToRunSectionType Type
        {
            get;
        }

        /// <inheritdoc />
        [MemberNotNullWhen(true, nameof(Contents))]
        public bool CanRead => Contents is IReadableSegment;

        /// <summary>
        /// Gets or sets the contents of the section.
        /// </summary>
        public ISegment Contents
        {
            get;
        }

        /// <inheritdoc />
        public override uint GetPhysicalSize() => Contents.GetPhysicalSize();

        /// <inheritdoc />
        public override void Write(BinaryStreamWriter writer) => Contents.Write(writer);

        /// <inheritdoc />
        public BinaryStreamReader CreateReader()
        {
            if (!CanRead)
                throw new InvalidOperationException("Contents of the ReadyToRun section is not readable.");
            return ((IReadableSegment) Contents).CreateReader();
        }
    }
}
