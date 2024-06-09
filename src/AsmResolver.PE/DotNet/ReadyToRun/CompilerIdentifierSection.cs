using System.Diagnostics;
using System.Text;
using AsmResolver.IO;

namespace AsmResolver.PE.DotNet.ReadyToRun
{
    /// <summary>
    /// Represents a section in a ReadyToRun .NET portable executable that contains an identifier of the compiler
    /// that was used to generate the image.
    /// </summary>
    [DebuggerDisplay("CompilerIdentifier ({" + nameof(Identifier) + "})")]
    public class CompilerIdentifierSection : SegmentBase, IReadyToRunSection
    {
        /// <summary>
        /// Creates a new compiler identifier section.
        /// </summary>
        /// <param name="identifier">The compiler identifier.</param>
        public CompilerIdentifierSection(string identifier)
        {
            Identifier = identifier;
        }

        /// <inheritdoc />
        public ReadyToRunSectionType Type => ReadyToRunSectionType.CompilerIdentifier;

        /// <inheritdoc />
        public bool CanRead => true;

        /// <summary>
        /// Gets or sets the identifier of the compiler that was used to create the ReadyToRun image.
        /// </summary>
        public string Identifier
        {
            get;
            set;
        }

        /// <inheritdoc />
        public BinaryStreamReader CreateReader() => new(Encoding.ASCII.GetBytes($"{Identifier}\0"));

        /// <inheritdoc />
        public override uint GetPhysicalSize() => (uint) Encoding.ASCII.GetByteCount(Identifier) + sizeof(byte);

        /// <inheritdoc />
        public override void Write(BinaryStreamWriter writer)
        {
            writer.WriteAsciiString(Identifier);
            writer.WriteByte(0);
        }
    }
}
