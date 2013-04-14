using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TUP.AsmResolver.NET.Specialized;

namespace TUP.AsmResolver.NET
{
    /// <summary>
    /// Represents a metadata row containing the raw values of a member.
    /// </summary>
    public class MetaDataRow
    {
        internal object[] parts;
        internal uint offset;

        public MetaDataRow()
        {
        }
        public MetaDataRow(params object[] parts)
        {
            this.parts = parts;
        }
        //public MetaDataRow(uint offset, params object[] parts)
        //{
        //    this.offset = offset;
        //    this.parts = parts;
        //}
        /// <summary>
        /// Gets the raw offset of the metadata row.
        /// </summary>
        public uint Offset
        {
            get { return offset; }
        }
        /// <summary>
        /// Gets the parts in array format.
        /// </summary>
        public object[] Parts
        { 
           get 
           { 
               return parts; 
           } 
        }
        /// <summary>
        /// Serializes the metadata row into a byte array.
        /// </summary>
        /// <returns></returns>
        public byte[] GenerateBytes()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryWriter writer = new BinaryWriter(stream);
                foreach (object part in parts)
                {
                    if (part is uint || part is int)
                        writer.Write((uint)part);
                    else if (part is ushort || part is short)
                        writer.Write((ushort)part);
                    else if (part is byte || part is sbyte)
                        writer.Write((byte)part);
                }
                return stream.ToArray();
            }
        }

    }
}
