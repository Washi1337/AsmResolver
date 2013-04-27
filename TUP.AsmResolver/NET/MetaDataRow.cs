using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TUP.AsmResolver.NET.Specialized;
using System.Runtime.InteropServices;

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
        /// Calculatest he size of the metadata row in bytes.
        /// </summary>
        /// <returns></returns>
        public int CalculateSize()
        {
            int size = 0;
            foreach (object part in parts)
                size += Marshal.SizeOf(part);
            return size;
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
