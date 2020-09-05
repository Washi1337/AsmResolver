using System;
using System.Collections.Generic;

namespace AsmResolver.PE.Win32Resources.Version
{
    /// <summary>
    /// Represents the organization of data in a file-version resource. It typically contains a list of language and
    /// code page identifier pairs that the version of the application or DLL supports.
    /// </summary>
    public class VarTable : VersionTableEntry
    {
        /// <summary>
        /// The name of the Var entry.
        /// </summary>
        public const string TranslationKey = "Translation";

        /// <summary>
        /// Reads a single Var table at the provided input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <returns>The var table.</returns>
        /// <exception cref="FormatException">
        /// Occurs when the input stream does not point to a valid Var table structure.
        /// </exception>
        public static VarTable FromReader(IBinaryStreamReader reader)
        {
            var header = VersionTableEntryHeader.FromReader(reader);
            if (header.Key != TranslationKey)
                throw new FormatException($"Expected a Var structure but got a {header.Key} structure.");

            reader.Align(4);
            
            var result = new VarTable();
            
            ulong start = reader.Offset;
            while (reader.Offset - start < header.ValueLength)
                result.Values.Add(reader.ReadUInt32());

            return result;
        }

        /// <inheritdoc />
        public override string Key => TranslationKey;

        /// <inheritdoc />
        protected override VersionTableValueType ValueType => VersionTableValueType.Binary;
        
        /// <summary>
        /// Gets a collection of one or more values that are language and code page identifier pairs. 
        /// </summary>
        public IList<uint> Values
        {
            get;
        } = new List<uint>();

        /// <inheritdoc />
        public override uint GetPhysicalSize()
        {
            uint size = VersionTableEntryHeader.GetHeaderSize(Key);
            size = size.Align(4);
            size += (uint) Values.Count * sizeof(uint);
            return size;
        }

        /// <inheritdoc />
        protected override uint GetValueLength() => (uint) (Values.Count * sizeof(uint));

        /// <inheritdoc />
        protected override void WriteValue(IBinaryStreamWriter writer)
        {
            foreach (uint value in Values)
                writer.WriteUInt32(value);
        }
    }
}