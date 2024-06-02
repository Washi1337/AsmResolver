using System;
using System.Text;
using AsmResolver.IO;

namespace AsmResolver.PE.Debug
{
    /// <summary>
    /// Represents the CodeView data in RSDS format
    /// </summary>
    public class RsdsDataSegment : CodeViewDataSegment
    {
        /// <summary>
        /// Gets the minimal expected data size for the rsds format
        /// </summary>
        private const uint RsdsExpectedDataSize =
                sizeof(uint)   // Signature
                + 16           // Guid
                + sizeof(uint) // Age
                + sizeof(byte) // Path
            ;

        /// <summary>
        /// Initializes a new instance of <see cref="RsdsDataSegment"/>
        /// </summary>
        /// <param name="context">Context for the reader</param>
        /// <param name="reader">The input stream to read from.</param>
        /// <returns></returns>
        public new static RsdsDataSegment? FromReader(PEReaderContext context, ref BinaryStreamReader reader)
        {
            if (reader.Length < RsdsExpectedDataSize)
            {
                context.BadImage("RSDS Data was shorter than the minimal expected length");
                return null;
            }

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
        public string? Path
        {
            get;
            set;
        }

        /// <inheritdoc />
        public override uint GetPhysicalSize()
        {
            return sizeof(uint)                 //Signature
                   + 16                         //Guid
                   + sizeof(uint)               //Age
                   + (uint) (Path?.Length ?? 0) //Path
                   + sizeof(byte)               //Zero byte for null terminated string
                ;
        }

        /// <inheritdoc />
        public override void Write(IBinaryStreamWriter writer)
        {
            writer.WriteUInt32((uint) Signature);
            writer.WriteBytes(Guid.ToByteArray());
            writer.WriteUInt32(Age);
            writer.WriteBytes(Encoding.UTF8.GetBytes(Path ?? string.Empty));
            writer.WriteByte(0x00);
        }
    }
}
