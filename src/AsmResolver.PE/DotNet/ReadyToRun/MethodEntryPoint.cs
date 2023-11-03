using System.Collections.Generic;
using AsmResolver.IO;

namespace AsmResolver.PE.DotNet.ReadyToRun
{
    /// <summary>
    /// Provides information about a native entry point for a managed method that was compiled ahead-of-time.
    /// </summary>
    public class MethodEntryPoint
    {
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
            uint header = NativeArrayView.DecodeUnsigned(ref reader);
            bool hasFixups = (header & 1) != 0;
            if (!hasFixups)
                return new MethodEntryPoint(header >> 1);

            var entryPoint = new MethodEntryPoint(header >> 2);
            ReadFixups(entryPoint, reader);
            return entryPoint;
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
    }
}
