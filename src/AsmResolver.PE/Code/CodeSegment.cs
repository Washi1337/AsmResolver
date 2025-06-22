using System;
using System.Collections.Generic;
using AsmResolver.IO;
using AsmResolver.Patching;

namespace AsmResolver.PE.Code
{
    /// <summary>
    /// Represents a chunk of native code in a portable executable.
    /// </summary>
    [Obsolete("This class has been superseded by AsmResolver.Patching.PatchedSegment.")]
    public class CodeSegment : SegmentBase
    {
        /// <summary>
        /// Creates a new segment of native code.
        /// </summary>
        /// <param name="code">The raw native code stream.</param>
        public CodeSegment(byte[] code)
        {
            Code = code;
        }

        /// <summary>
        /// Creates a new segment of native code.
        /// </summary>
        /// <param name="imageBase">The base address of the image the segment is going to be stored in.</param>
        /// <param name="code">The raw native code stream.</param>
        public CodeSegment(ulong imageBase, byte[] code)
        {
            ImageBase = imageBase;
            Code = code ?? throw new ArgumentNullException(nameof(code));
        }

        /// <summary>
        /// Gets the base address of the image the segment is stored in.
        /// </summary>
        public ulong ImageBase
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the raw native code stream.
        /// </summary>
        public byte[] Code
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a collection of fixups that need to be applied upon writing the code to the output stream.
        /// This includes addresses to imported symbols and global fields stored in data sections.
        /// </summary>
        public IList<AddressFixup> AddressFixups
        {
            get;
        } = new List<AddressFixup>();

        /// <inheritdoc />
        public override void UpdateOffsets(in RelocationParameters parameters)
        {
            ImageBase = parameters.ImageBase;
            base.UpdateOffsets(in parameters);
        }

        /// <inheritdoc />
        public override uint GetPhysicalSize() => (uint) Code.Length;

        /// <inheritdoc />
        public override void Write(BinaryStreamWriter writer)
        {
            ulong writerBase = writer.Offset;
            writer.WriteBytes(Code);

            for (int i = 0; i < AddressFixups.Count; i++)
                new AddressFixupPatch(AddressFixups[i]).Apply(new PatchContext(this, ImageBase, writerBase, writer));

            writer.Offset = Offset + GetPhysicalSize();
        }
    }
}
