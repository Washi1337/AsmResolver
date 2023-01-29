using System;
using System.Diagnostics;
using AsmResolver.Patching;

namespace AsmResolver.PE.Code
{
    /// <summary>
    /// Implements a patch that patches a segment with an address to a symbol.
    /// </summary>
    [DebuggerDisplay("{Fixup}")]
    public class AddressFixupPatch : IPatch
    {
        /// <summary>
        /// Creates a new instance of the <see cref="AddressFixupPatch"/> class.
        /// </summary>
        /// <param name="fixup">The fixup to apply.</param>
        public AddressFixupPatch(AddressFixup fixup)
        {
            Fixup = fixup;
        }

        /// <summary>
        /// Gets the fixup to apply.
        /// </summary>
        public AddressFixup Fixup
        {
            get;
        }

        /// <inheritdoc />
        public void Apply(in PatchContext context)
        {
            context.Writer.Offset = context.Segment.Offset + Fixup.Offset;
            uint writerRva = context.Segment.Rva + Fixup.Offset;
            uint targetRva = Fixup.Symbol.GetReference()?.Rva ?? 0;

            switch (Fixup.Type)
            {
                case AddressFixupType.Absolute32BitAddress:
                    context.Writer.WriteUInt32((uint) (context.ImageBase + targetRva));
                    break;

                case AddressFixupType.Relative32BitAddress:
                    context.Writer.WriteInt32((int) (targetRva - (writerRva + 4)));
                    break;

                case AddressFixupType.Absolute64BitAddress:
                    context.Writer.WriteUInt64(context.ImageBase + targetRva);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
