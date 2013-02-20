using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace TUP.AsmResolver.NET
{
    /// <summary>
    /// A heap that holds all user specified strings in method bodies.
    /// </summary>
    public class UserStringsHeap : Heap
    {
        internal UserStringsHeap(MetaDataStream stream)
            : base(stream)
        {
        }

        Dictionary<uint, string> readStrings = new Dictionary<uint, string>();
        BinaryReader binaryreader;

        public static UserStringsHeap FromStream(MetaDataStream stream)
        {
            UserStringsHeap heap = new UserStringsHeap(stream);
            heap.binaryreader = new BinaryReader(new MemoryStream(heap.Contents));
            return heap;
        }

        /// <summary>
        /// Gets a string by its offset.
        /// </summary>
        /// <param name="offset">The offset of the string.</param>
        /// <returns></returns>
        public string GetStringByOffset(uint offset)
        {
            string stringValue;
            if (readStrings.TryGetValue(offset, out stringValue))
                return stringValue;

            binaryreader.BaseStream.Seek(offset, SeekOrigin.Begin);

            uint length = (uint)(ReadCompressedUInt32() & -2);
            if (length < 0x1)
                return string.Empty;
            
            char[] chars = new char[length / 2];

            for (int i = 0; i < length; i += 2)
                chars[i / 2] = (char)binaryreader.ReadInt16();
            

            stringValue = new string(chars);
            readStrings.Add(offset, stringValue);
            return stringValue;


        }

        private uint ReadCompressedUInt32()
        {
            // stream.Seek(index, SeekOrigin.Begin);
            byte num = binaryreader.ReadByte();
            if ((num & 0x80) == 0)
            {
                return num;
            }
            if ((num & 0x40) == 0)
            {
                return (uint)(((num & -129) << 8) | binaryreader.ReadByte());
            }
            return (uint)(((((num & -193) << 0x18) | (binaryreader.ReadByte() << 0x10)) | (binaryreader.ReadByte() << 8)) | binaryreader.ReadByte());
        }
        /// <summary>
        /// Frees all the streams that are being used in this heap.
        /// </summary>
        public override void Dispose()
        {
            binaryreader.BaseStream.Close();
            binaryreader.BaseStream.Dispose();
            binaryreader.Close();
            binaryreader.Dispose();
        }
    }
}
