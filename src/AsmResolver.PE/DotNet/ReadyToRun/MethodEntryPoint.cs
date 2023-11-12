using System.Collections.Generic;
using System.IO;
using AsmResolver.IO;

namespace AsmResolver.PE.DotNet.ReadyToRun
{
    /// <summary>
    /// Provides information about a native entry point for a managed method that was compiled ahead-of-time.
    /// </summary>
    public class MethodEntryPoint : SegmentBase
    {
        private byte[]? _serialized;

        /// <summary>
        /// Constructs a new entry point for a method.
        /// </summary>
        /// <param name="functionIndex">The index of the <c>RUNTIME_FUNCTION</c> this method starts at.</param>
        public MethodEntryPoint(uint functionIndex)
        {
            RuntimeFunctionIndex = functionIndex;
        }

        /// <summary>
        /// Gets or sets the index to the <c>RUNTIME_FUNCTION</c> the method starts at.
        /// </summary>
        public uint RuntimeFunctionIndex
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a collection of fixups that need to be applied before the method can be executed natively.
        /// </summary>
        public IList<MethodFixup> Fixups
        {
            get;
        } = new List<MethodFixup>();

        /// <summary>
        /// Reads a single method entry point metadata segment from the provided input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <returns>The read entry point metadata</returns>
        public static MethodEntryPoint FromReader(ref BinaryStreamReader reader)
        {
            ulong offset = reader.Offset;
            uint rva = reader.Rva;

            uint header = NativeFormat.DecodeUnsigned(ref reader);
            bool hasFixups = (header & 1) != 0;

            MethodEntryPoint result;
            if (!hasFixups)
            {
                result = new MethodEntryPoint(header >> 1);
            }
            else
            {
                result = new MethodEntryPoint(header >> 2);
                ReadFixups(result, reader);
            }

            result.Offset = offset;
            result.Rva = rva;

            return result;
        }

        private static void ReadFixups(MethodEntryPoint entryPoint, BinaryStreamReader reader)
        {
            var nibbleReader = new NibbleReader(reader);

            uint importIndex = nibbleReader.Read3BitEncodedUInt();
            while (true)
            {
                uint slotIndex = nibbleReader.Read3BitEncodedUInt();
                while (true)
                {
                    entryPoint.Fixups.Add(new MethodFixup(importIndex, slotIndex));

                    uint slotDelta = nibbleReader.Read3BitEncodedUInt();
                    if (slotDelta == 0)
                        break;

                    slotIndex += slotDelta;
                }

                uint importDelta = nibbleReader.Read3BitEncodedUInt();
                if (importDelta == 0)
                    break;

                importIndex += importDelta;
            }
        }

        private uint GetHeader() => Fixups.Count > 0
            ? RuntimeFunctionIndex << 2 | 1
            : RuntimeFunctionIndex << 1;

        /// <inheritdoc />
        public override void UpdateOffsets(in RelocationParameters parameters)
        {
            base.UpdateOffsets(in parameters);
            _serialized = Serialize();
        }

        /// <inheritdoc />
        public override uint GetPhysicalSize()
        {
            _serialized ??= Serialize();
            return (uint) _serialized.Length;
        }

        /// <inheritdoc />
        public override void Write(IBinaryStreamWriter writer)
        {
            _serialized ??= Serialize();
            writer.WriteBytes(_serialized);
        }

        private byte[] Serialize()
        {
            using var stream = new MemoryStream();
            var writer = new BinaryStreamWriter(stream);

            // Write header.
            NativeFormat.EncodeUnsigned(writer, GetHeader());

            if (Fixups.Count == 0)
                return stream.ToArray();

            var nibbleWriter = new NibbleWriter(writer);

            // Write fixups.
            uint lastImportIndex = 0;
            uint lastSlotIndex = 0;
            for (int i = 0; i < Fixups.Count; i++)
            {
                var fixup = Fixups[i];

                uint importDelta = fixup.ImportIndex - lastImportIndex;
                if (importDelta != 0 || i == 0)
                {
                    // We're entering a new chunk of fixups with a different import index.
                    // Close of previous import chunk with a 0 slot delta.
                    if (i > 0)
                        nibbleWriter.Write3BitEncodedUInt(0);

                    // Start new import chunk.
                    nibbleWriter.Write3BitEncodedUInt(importDelta);
                    lastSlotIndex = 0;
                }

                // Write current slot.
                uint slotDelta = fixup.SlotIndex - lastSlotIndex;
                nibbleWriter.Write3BitEncodedUInt(slotDelta);

                lastImportIndex = fixup.ImportIndex;
                lastSlotIndex = fixup.SlotIndex;
            }

            // Close off last slot list.
            nibbleWriter.Write3BitEncodedUInt(0);

            // Close off last import list.
            nibbleWriter.Write3BitEncodedUInt(0);

            nibbleWriter.Flush();

            return stream.ToArray();
        }
    }
}
