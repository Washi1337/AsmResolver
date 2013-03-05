using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TUP.AsmResolver.PE;

namespace TUP.AsmResolver.NET
{
    /// <summary>
    /// A heap that holds all user specified strings in method bodies.
    /// </summary>
    public class UserStringsHeap : MetaDataStream
    {
        uint newEntryOffset = 0;
        bool hasReadAllStrings = false;
        SortedDictionary<uint, string> readStrings = new SortedDictionary<uint, string>();

        internal UserStringsHeap(NETHeader netheader, int headeroffset, Structures.METADATA_STREAM_HEADER rawHeader, string name)
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
        //        byte[] bytes = Encoding.Unicode.GetBytes(readString.Value);
        //        NETGlobals.WriteCompressedUInt32(writer, (uint)bytes.Length + 1); // length + terminator length
        //        writer.Write(bytes); // data
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

            uint lastPosition = (uint)mainStream.Position;
            while (mainStream.Position + 1 < mainStream.Length)
            {
                // TODO: write string.empty strings..

                bool alreadyExisted = readStrings.ContainsKey((uint)mainStream.Position + 1);
                string value = GetStringByOffset((uint)mainStream.Position + 1);


                int length = value.Length * 2;
                if (length == 0 && lastPosition == (uint)mainStream.Position)
                    mainStream.Seek(1, SeekOrigin.Current);
                if (alreadyExisted)
                    mainStream.Seek(length + NETGlobals.GetCompressedUInt32Size((uint)length) + 1, SeekOrigin.Current);

                lastPosition = (uint)mainStream.Position;
            }

            hasReadAllStrings = true;
            newEntryOffset = (uint)mainStream.Length;
        }

        /// <summary>
        /// Frees all the streams that are being used in this heap.
        /// </summary>
        public override void Dispose()
        {
            binReader.BaseStream.Close();
            binReader.BaseStream.Dispose();
            binReader.Close();
            binReader.Dispose();
        }

        public override void ClearCache()
        {
            readStrings.Clear();
            hasReadAllStrings = false;
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

            mainStream.Seek(offset, SeekOrigin.Begin);

            uint length = (uint)(NETGlobals.ReadCompressedUInt32(binReader) & -2);
            if (length == 0)
            {
                readStrings.Add(offset, string.Empty);
                return string.Empty;
            }
            
            char[] chars = new char[length / 2];

            for (int i = 0; i < length; i += 2)
                chars[i / 2] = (char)binReader.ReadInt16();
            

            stringValue = new string(chars);
            readStrings.Add(offset, stringValue);
            return stringValue;
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
            mainStream.Seek(offset, SeekOrigin.Begin);
            binWriter.Write(Encoding.Unicode.GetBytes(value));
            newEntryOffset += (uint)(value.Length + 1);
            readStrings.Add(offset, value);

            streamHeader.Size = (uint)mainStream.Length;

            return offset;
        }
    }
}
