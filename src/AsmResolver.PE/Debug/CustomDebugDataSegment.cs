using System;

namespace AsmResolver.PE.Debug
{
    /// <summary>
    /// Represents a debug data stream with a custom or unsupported format, wrapping an instance of <see cref="ISegment"/>
    /// into a <see cref="IDebugDataSegment"/>.
    /// </summary>
    public class CustomDebugDataSegment : IDebugDataSegment
    {
        /// <summary>
        /// Creates a new instance of the <see cref="CustomDebugDataSegment"/> class. 
        /// </summary>
        /// <param name="type">The format of the data.</param>
        /// <param name="contents">The contents of the code.</param>
        public CustomDebugDataSegment(DebugDataType type, ISegment contents)
        {
            Type = type;
            Contents = contents ?? throw new ArgumentNullException(nameof(contents));
        }
        
        /// <inheritdoc />
        public DebugDataType Type
        {
            get;
        }

        /// <summary>
        /// Gets or sets the raw data of the segment.
        /// </summary>
        public ISegment Contents
        {
            get;
            set;
        }

        /// <inheritdoc />
        public ulong Offset => Contents?.Offset ?? 0;

        /// <inheritdoc />
        public uint Rva => Contents?.Rva ?? 0;

        /// <inheritdoc />
        public bool CanUpdateOffsets => Contents?.CanUpdateOffsets ?? false;

        /// <inheritdoc />
        public void UpdateOffsets(ulong newOffset, uint newRva)
        {
            if (Contents != null)
                Contents.UpdateOffsets(newOffset, newRva);
            else
                throw new ArgumentNullException(nameof(Contents));
        }
        
        /// <inheritdoc />
        public uint GetPhysicalSize() => Contents?.GetPhysicalSize() ?? 0;

        /// <inheritdoc />
        public uint GetVirtualSize() => Contents?.GetPhysicalSize() ?? 0;
        
        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer) => Contents?.Write(writer);
    }
}