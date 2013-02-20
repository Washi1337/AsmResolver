using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
namespace TUP.AsmResolver.NET
{
    /// <summary>
    /// A heap that holds all string values of all members in the tables heap.
    /// </summary>
    public class StringsHeap : Heap
    {
        internal StringsHeap(MetaDataStream stream)
            : base(stream)
        {
        }
        Dictionary<uint, string> readStrings = new Dictionary<uint, string>();

        BinaryReader binaryreader;
        public static StringsHeap FromStream(MetaDataStream stream)
        {
            StringsHeap heap = new StringsHeap(stream);

            heap.binaryreader = new BinaryReader(new MemoryStream(heap.Contents));
            return heap;
        }

        /// <summary>
        /// Gets the string by its offset.
        /// </summary>
        /// <param name="offset">The offset of the string.</param>
        /// <returns></returns>
        public string GetStringByOffset(uint offset)
        {
            string stringValue;
            if (readStrings.TryGetValue(offset, out stringValue))
                return stringValue;

            binaryreader.BaseStream.Seek(offset, SeekOrigin.Begin);
            byte lastByte = 0;
            do
            {
                lastByte = binaryreader.ReadByte();

            } while (lastByte != 0);

            int endoffset = (int)binaryreader.BaseStream.Position - 1;

            binaryreader.BaseStream.Seek(offset, SeekOrigin.Begin);

            stringValue = Encoding.UTF8.GetString(binaryreader.ReadBytes(endoffset - (int)offset), 0, endoffset - (int)offset);
            readStrings.Add(offset, stringValue);
            return readStrings[offset];
        }


        /// <summary>
        /// Frees all the streams used in this heap.
        /// </summary>
        public override void Dispose()
        {
            readStrings.Clear();
            binaryreader.BaseStream.Close();
            binaryreader.BaseStream.Dispose();
            binaryreader.Close();
            binaryreader.Dispose();
        }
    }
}
