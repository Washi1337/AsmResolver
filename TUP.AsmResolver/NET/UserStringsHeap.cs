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
        uint _newEntryOffset = 0;
        bool _hasReadAllStrings = false;
        SortedDictionary<uint, string> _readStrings = new SortedDictionary<uint, string>();

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
            _mainStream.Seek(0, SeekOrigin.Begin);

            uint lastPosition = (uint)_mainStream.Position;
            while (_mainStream.Position + 1 < _mainStream.Length)
            {
                // TODO: write string.empty strings..

                bool alreadyExisted = _readStrings.ContainsKey((uint)_mainStream.Position + 1);
                string value = GetStringByOffset((uint)_mainStream.Position + 1);


                int length = value.Length * 2;
                if (length == 0 && lastPosition == (uint)_mainStream.Position)
                    _mainStream.Seek(1, SeekOrigin.Current);
                if (alreadyExisted)
                    _mainStream.Seek(length + NETGlobals.GetCompressedUInt32Size((uint)length) + 1, SeekOrigin.Current);

                lastPosition = (uint)_mainStream.Position;
            }

            _hasReadAllStrings = true;
            _newEntryOffset = (uint)_mainStream.Length;
        }

        /// <summary>
        /// Frees all the streams that are being used in this heap.
        /// </summary>
        public override void Dispose()
        {
            _binReader.BaseStream.Close();
            _binReader.BaseStream.Dispose();
            _binReader.Close();
            _binReader.Dispose();
        }

        public override void ClearCache()
        {
            _readStrings.Clear();
            _hasReadAllStrings = false;
        }

        /// <summary>
        /// Gets a string by its offset.
        /// </summary>
        /// <param name="offset">The offset of the string.</param>
        /// <returns></returns>
        public string GetStringByOffset(uint offset)
        {
            string stringValue;
            if (_readStrings.TryGetValue(offset, out stringValue))
                return stringValue;

            _mainStream.Seek(offset, SeekOrigin.Begin);

            uint length = (uint)(NETGlobals.ReadCompressedUInt32(_binReader) & -2);
            if (length == 0)
            {
                _readStrings.Add(offset, string.Empty);
                return string.Empty;
            }
            
            char[] chars = new char[length / 2];

            for (int i = 0; i < length; i += 2)
                chars[i / 2] = (char)_binReader.ReadInt16();
            

            stringValue = new string(chars);
            _readStrings.Add(offset, stringValue);
            return stringValue;
        }

        /// <summary>
        /// Gets an offset of a string value. If it is not present in the strings heap, it will add it.
        /// </summary>
        /// <param name="value">The string value to get the offset from.</param>
        /// <returns></returns>
        public uint GetStringOffset(string value)
        {
            if (!_hasReadAllStrings)
                ReadAllStrings();

            if (_readStrings.ContainsValue(value))
                return _readStrings.First(rs => rs.Value == value).Key;

            uint offset = _newEntryOffset;
            _mainStream.Seek(offset, SeekOrigin.Begin);
            _binWriter.Write(Encoding.Unicode.GetBytes(value));
            _newEntryOffset += (uint)(value.Length + 1);
            _readStrings.Add(offset, value);

            _streamHeader.Size = (uint)_mainStream.Length;

            return offset;
        }
    }
}
