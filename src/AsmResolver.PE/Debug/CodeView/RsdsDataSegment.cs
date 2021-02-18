using System;
using System.Text;

namespace AsmResolver.PE.Debug.CodeView
{
    /// <summary>
    /// Represents the CodeView data in RSDS format
    /// </summary>
    public class RsdsDataSegment : CodeViewDataSegment
    {

        /// <summary>
        /// Initializes a new instance of <see cref="RsdsDataSegment"/>
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public new static RsdsDataSegment FromReader(IBinaryStreamReader reader)
        {
            var result = new RsdsDataSegment();
            byte[] buffer = new byte[16];
            reader.ReadBytes(buffer, 0, 16);
            result.Guid = new Guid(buffer);
            result.Age = reader.ReadUInt32();
            result.Path = Encoding.UTF8.GetString(reader.ReadBytesUntil(0x00));

            return result;
        }

        /// <summary>
        /// Gets the PDB Signature
        /// </summary>
        public override CodeViewSignature Signature => CodeViewSignature.Rsds;

        /// <summary>
        /// Gets or sets the PDB GUID
        /// </summary>
        public Guid Guid
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
            writer.WriteUInt32((uint)Signature);
            writer.WriteBytes(Guid.ToByteArray());
            writer.WriteUInt32(Age);
            writer.WriteBytes(Encoding.UTF8.GetBytes(Path));
        }
    }
}
