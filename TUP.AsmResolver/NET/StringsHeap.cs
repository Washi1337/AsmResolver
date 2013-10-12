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
        bool _hasReadAllStrings = false;
        SortedDictionary<uint, string> _readStrings = new SortedDictionary<uint, string>();

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
            _mainStream.Seek(0, SeekOrigin.Begin);
            while (_mainStream.Position + 1 < _mainStream.Length)
            {
                bool alreadyExisted = _readStrings.ContainsKey((uint)_mainStream.Position + 1);
                string value = GetStringByOffset((uint)_mainStream.Position + 1);

                int length = Encoding.UTF8.GetBytes(value).Length;
                if (length == 0)
                    _mainStream.Seek(1, SeekOrigin.Current);
                if (alreadyExisted)
                    _mainStream.Seek(length + 1, SeekOrigin.Current);
            }
            _hasReadAllStrings = true;
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
            _readStrings.Clear();
            _hasReadAllStrings = false;
        }

        public bool TryGetStringByOffset(uint offset, out string value)
        {
            value = string.Empty;

            if (offset == 0 || offset >= _mainStream.Length)
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
            if (_readStrings.TryGetValue(offset, out stringValue))
                return stringValue;

            if (_mainStream.Position > _mainStream.Length)
                throw new EndOfStreamException();

            _mainStream.Seek(offset, SeekOrigin.Begin);
            byte lastByte = 0;
            do
            {
                lastByte = _binReader.ReadByte();

            } while (lastByte != 0 && _mainStream.Position < _mainStream.Length);

            int endoffset = (int)_mainStream.Position - 1;

            _mainStream.Seek(offset, SeekOrigin.Begin);

            stringValue = Encoding.UTF8.GetString(_binReader.ReadBytes(endoffset - (int)offset), 0, endoffset - (int)offset);
            _readStrings.Add(offset, stringValue);
            return _readStrings[offset];
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

            if (!_hasReadAllStrings)
                ReadAllStrings();

            if (_readStrings.ContainsValue(value))
                return _readStrings.First(rs => rs.Value == value).Key;

            uint offset = (uint)_mainStream.Length;
            _mainStream.Seek(offset, SeekOrigin.Begin);
            _binWriter.Write(Encoding.UTF8.GetBytes(value)); // data
            _binWriter.Write((byte)0); // terminator

            _readStrings.Add(offset, value);

            _streamHeader.Size = (uint)_mainStream.Length;
            
            return offset;
        }

    }
}
