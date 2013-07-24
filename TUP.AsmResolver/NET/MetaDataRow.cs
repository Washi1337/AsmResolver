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
    public struct MetaDataRow
    {
        internal ValueType[] _parts;
        internal uint _offset;
        internal NETHeader _netHeader;

        public MetaDataRow(params ValueType[] parts)
        {
            this._parts = parts;
            this._offset = 0;
            this._netHeader = null;
        }

        public MetaDataRow(uint offset, params ValueType[] parts)
        {
            this._offset = offset;
            this._parts = parts;
            this._netHeader = null;
        }

        public NETHeader NETHeader
        {
            get { return _netHeader; }
            set { _netHeader = value; }
        }

        /// <summary>
        /// Gets the raw offset of the metadata row.
        /// </summary>
        public uint Offset
        {
            get { return _offset; }
            set { _offset = value; }
        }

        /// <summary>
        /// Gets the parts in array format.
        /// </summary>
        public ValueType[] Parts
        { 
            get 
            { 
                return _parts; 
            }
            set
            {
                if (value.Length != _parts.Length)
                {
                    throw new NotSupportedException("Value must have the same length.");
                }
                _parts = value;
            }
        }

        /// <summary>
        /// Calculatest he size of the metadata row in bytes.
        /// </summary>
        /// <returns></returns>
        public int CalculateSize()
        {
            int size = 0;
            foreach (object part in _parts)
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
                foreach (object part in _parts)
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

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            if (Offset != 0)
            {
                builder.Append(string.Format("0x{0:X}:", Offset));
            }

            builder.Append("{");
            
            for (int i = 0; i < _parts.Length; i++)
            {
                builder.Append(string.Format("0x{0:X}{1}", _parts[i], i == _parts.Length - 1 ? "" : ", "));
            }
            
            
            builder.Append("}");
            return builder.ToString();
        }
    }
}
