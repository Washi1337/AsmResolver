using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TUP.AsmResolver.PE;
namespace TUP.AsmResolver.NET
{
    /// <summary>
    /// A heap that holds all string values of all members in the tables heap.
    /// </summary>
    public class StringsHeap : MetaDataStream
    {
        bool hasReadAllStrings = false;
        uint newEntryOffset = 0;
        SortedDictionary<uint, string> readStrings = new SortedDictionary<uint, string>();
        MemoryStream stream;
        BinaryReader binaryreader;

        internal StringsHeap(NETHeader netheader, int headeroffset, Structures.METADATA_STREAM_HEADER rawHeader, string name)
            : base(netheader, headeroffset, rawHeader, name)
        {
            stream = new MemoryStream(this.Contents);
            binaryreader = new BinaryReader(stream);
        }

        internal override void Initialize()
        {
        }

        internal override void Reconstruct()
        {
            MemoryStream newStream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(newStream);
            writer.Write((byte)0);
            ReadAllStrings();
            foreach (var readString in readStrings)
            {
                writer.Write(Encoding.UTF8.GetBytes(readString.Value)); // data
                writer.Write((byte)0); // terminator
            }
            binaryreader.Dispose();
            stream.Dispose();
            stream = newStream;
            binaryreader = new BinaryReader(newStream);
            this.streamHeader.Size = (uint)newStream.Length;
            this.contents = newStream.ToArray();
        }

        internal void ReadAllStrings()
        {
            stream.Seek(0, SeekOrigin.Begin);
            while (stream.Position + 1 < stream.Length)
            {
                bool alreadyExisted = readStrings.ContainsKey((uint)stream.Position + 1);
                string value = GetStringByOffset((uint)stream.Position + 1);

                int length = Encoding.UTF8.GetBytes(value).Length;
                if (length == 0)
                    stream.Seek(1, SeekOrigin.Current);
                if (alreadyExisted)
                    stream.Seek(length + 1, SeekOrigin.Current);
            }
            hasReadAllStrings = true;
            newEntryOffset = (uint)stream.Length;
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
            

            stream.Seek(offset, SeekOrigin.Begin);
            byte lastByte = 0;
            do
            {
                lastByte = binaryreader.ReadByte();

            } while (lastByte != 0);

            int endoffset = (int)stream.Position - 1;

            stream.Seek(offset, SeekOrigin.Begin);

            stringValue = Encoding.UTF8.GetString(binaryreader.ReadBytes(endoffset - (int)offset), 0, endoffset - (int)offset);
            readStrings.Add(offset, stringValue);
            return readStrings[offset];
        }
        /// <summary>
        /// Gets an offset of a string value. If it is not present in the strings heap, it will add it.
        /// </summary>
        /// <param name="value">The string value to get the offset from.</param>
        /// <returns></returns>
        public uint GetStringOffset(string value)
        {
            if (!hasReadAllStrings)
                ReadAllStrings();

            if (readStrings.ContainsValue(value))
                return readStrings.First(rs => rs.Value == value).Key;

            uint offset = newEntryOffset;
            readStrings.Add(offset, value);
            newEntryOffset += (uint)(value.Length + 1);
            return offset;
        }
    }
}
