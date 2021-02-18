using System;
using System.Text;

namespace AsmResolver.PE.Debug
{
    /// <summary>
    /// Represents a debug data stream with CodeView format, wrapping an instance of <see cref="ISegment"/>
    /// into a <see cref="IDebugDataSegment"/>.
    /// </summary>
    public class CodeViewDebugDataSegment : SegmentBase, IDebugDataSegment
    {
        /// <summary>
        /// Creates a new instance of the <see cref="CustomDebugDataSegment"/> class.
        /// </summary>
        /// <param name="type">The format of the data.</param>
        /// <param name="reader">BinaryStreamReader for the segment contents</param>
        public CodeViewDebugDataSegment(DebugDataType type, IBinaryStreamReader reader)
        {
            Type = type;
            Signature = reader.ReadUInt32();
            byte[] buffer = new byte[16];
            reader.ReadBytes(buffer, 0, 16);
            GUID = new Guid(buffer);
            Age = reader.ReadUInt32();
            Path = Encoding.UTF8.GetString(reader.ReadBytesUntil(0x00));
        }

        /// <inheritdoc />
        public DebugDataType Type
        {
            get;
        }

        /// <summary>
        /// Gets or sets the PDB Signature
        /// </summary>
        public uint Signature
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the PDB GUID
        /// </summary>
        public Guid GUID
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the PDB age
        /// </summary>
        public uint Age
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the PDB path
        /// </summary>
        public string Path
        {
            get;
            set;
        }


        /// <inheritdoc />
        public override uint GetPhysicalSize()
        {
            return sizeof(uint)                    //Signature
                   + 16                            //Guid
                   + sizeof(uint)                  //Age
                   + ((uint) Path.Length).Align(4) //Path
                ;
        }

        /// <inheritdoc />
        public override void Write(IBinaryStreamWriter writer)
        {
            writer.WriteUInt32(Signature);
            writer.WriteBytes(GUID.ToByteArray());
            writer.WriteUInt32(Age);
            writer.WriteBytes(Encoding.UTF8.GetBytes(Path));
        }
    }
}
