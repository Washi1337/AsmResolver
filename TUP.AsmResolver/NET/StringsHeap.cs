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
        SortedDictionary<uint, string> readStrings = new SortedDictionary<uint, string>();

        internal StringsHeap(NETHeader netheader, int headeroffset, Structures.METADATA_STREAM_HEADER rawHeader, string name)
            : base(netheader, headeroffset, rawHeader, name)
        {
        }

        internal override void Initialize()
        {
        }

        //internal override void Reconstruct()
        //{
        //    MemoryStream newStream = new MemoryStream();
        //    BinaryWriter writer = new BinaryWriter(newStream);
        //    writer.Write((byte)0);
        //    ReadAllStrings();
        //    foreach (var readString in readStrings)
        //    {
        //        writer.Write(Encoding.UTF8.GetBytes(readString.Value)); // data
        //        writer.Write((byte)0); // terminator
        //    }
        //    binaryreader.Dispose();
        //    stream.Dispose();
        //    stream = newStream;
        //    binaryreader = new BinaryReader(newStream);
        //    this.streamHeader.Size = (uint)newStream.Length;
        //    this.contents = newStream.ToArray();
        //}

        internal void ReadAllStrings()
        {
            mainStream.Seek(0, SeekOrigin.Begin);
            while (mainStream.Position + 1 < mainStream.Length)
            {
                bool alreadyExisted = readStrings.ContainsKey((uint)mainStream.Position + 1);
                string value = GetStringByOffset((uint)mainStream.Position + 1);

                int length = Encoding.UTF8.GetBytes(value).Length;
                if (length == 0)
                    mainStream.Seek(1, SeekOrigin.Current);
                if (alreadyExisted)
                    mainStream.Seek(length + 1, SeekOrigin.Current);
            }
            hasReadAllStrings = true;
        }

        /// <summary>
        /// Frees all the streams used in this heap.
        /// </summary>
        public override void Dispose()
        {
            ClearCache();
            base.Dispose();
        }

        public override void ClearCache()
        {
            readStrings.Clear();
            hasReadAllStrings = false;
        }

        public bool TryGetStringByOffset(uint offset, out string value)
        {
            value = string.Empty;

            if (offset > mainStream.Length)
                return false;

            value = GetStringByOffset(offset);
            return true;
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

            if (mainStream.Position > mainStream.Length)
                throw new EndOfStreamException();

            mainStream.Seek(offset, SeekOrigin.Begin);
            byte lastByte = 0;
            do
            {
                lastByte = binReader.ReadByte();

            } while (lastByte != 0 && mainStream.Position < mainStream.Length);

            int endoffset = (int)mainStream.Position - 1;

            mainStream.Seek(offset, SeekOrigin.Begin);

            stringValue = Encoding.UTF8.GetString(binReader.ReadBytes(endoffset - (int)offset), 0, endoffset - (int)offset);
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
            if (string.IsNullOrEmpty(value))
                return 0;

            if (!hasReadAllStrings)
                ReadAllStrings();

            if (readStrings.ContainsValue(value))
                return readStrings.First(rs => rs.Value == value).Key;

            uint offset = (uint)mainStream.Length;
            mainStream.Seek(offset, SeekOrigin.Begin);
            binWriter.Write(Encoding.UTF8.GetBytes(value)); // data
            binWriter.Write((byte)0); // terminator

            readStrings.Add(offset, value);

            streamHeader.Size = (uint)mainStream.Length;
            
            return offset;
        }

    }
}
