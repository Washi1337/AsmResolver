using System;

namespace AsmResolver.PE.Debug
{
    /// <summary>
    /// Provides an implementation of a debug data entry that was stored in a PE file.
    /// </summary>
    public class SerializedDebugDataEntry : DebugDataEntry
    {
        private readonly IDebugDataReader _dataReader;
        private readonly DebugDataType _type;
        private readonly uint _sizeOfData;
        private readonly uint _addressOfRawData;
        private readonly uint _pointerToRawData;

        /// <summary>
        /// Reads a single debug data entry from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <param name="dataReader">The object responsible for reading the contents.</param>
        public SerializedDebugDataEntry(IBinaryStreamReader reader, IDebugDataReader dataReader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            _dataReader = dataReader ?? throw new ArgumentNullException(nameof(dataReader));
            Offset = reader.Offset;
            Rva = reader.Rva;
            
            Characteristics = reader.ReadUInt32();
            TimeDateStamp = reader.ReadUInt32();
            MajorVersion = reader.ReadUInt16();
            MinorVersion = reader.ReadUInt16();
            _type = (DebugDataType) reader.ReadUInt32();
            _sizeOfData = reader.ReadUInt32();
            _addressOfRawData = reader.ReadUInt32();
            _pointerToRawData = reader.ReadUInt32();
        }
        
        /// <inheritdoc />
        protected override IDebugDataSegment GetContents() => 
            _dataReader.ReadDebugData(_type, _addressOfRawData, _sizeOfData);
    }
}