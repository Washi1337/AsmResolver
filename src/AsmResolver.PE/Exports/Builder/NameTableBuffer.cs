using System.Collections.Generic;
using System.Text;

namespace AsmResolver.PE.Exports.Builder
{
    /// <summary>
    /// Represents a mutable buffer that stores strings referenced by exported symbols in the export data directory.
    /// </summary>
    public class NameTableBuffer : SegmentBase
    {
        private readonly List<string> _entries = new List<string>();
        private readonly IDictionary<string, uint> _nameOffsets = new Dictionary<string, uint>();
        private uint _length;

        /// <summary>
        /// Adds the provided name to the buffer if it does not exist yet.
        /// </summary>
        /// <param name="name">The name to add.</param>
        public void AddName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return;
            
            if (!_nameOffsets.ContainsKey(name))
            {
                // Register string.
                _entries.Add(name);
                _nameOffsets.Add(name, _length);
                
                // Calculate length + zero terminator.
                _length += (uint) Encoding.ASCII.GetByteCount(name) + 1u;
            }
        }

        /// <summary>
        /// When the name was registered in the buffer, obtains the relative virtual address (RVA) to the name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The RVA.</returns> 
        /// <remarks>
        /// This method should only be used after the hint-name table has been relocated to the right location in the
        /// PE file.
        /// </remarks>
        public uint GetNameRva(string name) => Rva + _nameOffsets[name];

        /// <inheritdoc />
        public override uint GetPhysicalSize() => _length;

        /// <inheritdoc />
        public override void Write(IBinaryStreamWriter writer)
        {
            foreach (string value in _entries)
            {
                writer.WriteAsciiString(value);
                writer.WriteByte(0);
            }
        }
        
    }
}