using System.Collections.Generic;
using System.Linq;
using AsmResolver.IO;

namespace AsmResolver.PE.DotNet.VTableFixups.Builder
{
    /// <summary>
    /// Provides a mechanism for building the VTable Fixups entries in a PE file.
    /// </summary>
    public class VTableEntriesBuffer : SegmentBase
    {
        private readonly List<VTableFixup> _vtables = new();

        /// <summary>
        /// The default constructor
        /// </summary>
        public VTableEntriesBuffer()
        {

        }

        /// <summary>
        /// Creates an instance supplied with existing VTable Fixups
        /// </summary>
        /// <param name="vTableFixups"></param>
        public VTableEntriesBuffer(IEnumerable<VTableFixup> vTableFixups)
        {
            _vtables.AddRange(vTableFixups);
        }

        /// <summary>
        /// Adds a single VTable Fixup to the buffer.
        /// </summary>
        /// <param name="vTableFixup">The VTable Fixup to add.</param>
        public void Add(VTableFixup vTableFixup)
        {
            _vtables.Add(vTableFixup);
        }

        /// <inheritdoc />
        public override uint GetPhysicalSize() => (uint)_vtables.Sum(v => GetEntriesSize(v));

        /// <inheritdoc />
        public override void Write(IBinaryStreamWriter writer)
        {
            foreach (var vTable in _vtables)
            {
                foreach (var token in vTable.Tokens)
                {
                    if (vTable.Type.HasFlag(VTableType.VTable32Bit))
                        writer.WriteUInt32(token.ToUInt32());
                    else
                        writer.WriteUInt64(token.ToUInt32());
                }
            }
        }

        private static uint GetEntriesSize(VTableFixup vTableFixup) =>
            (uint)vTableFixup.Tokens.Count *
            (uint)(vTableFixup.Type.HasFlag(VTableType.VTable32Bit)
                ? 4
                : 8);
    }
}
