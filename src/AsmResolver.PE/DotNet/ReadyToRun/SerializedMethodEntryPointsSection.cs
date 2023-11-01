using System.Collections.Generic;
using AsmResolver.IO;

namespace AsmResolver.PE.DotNet.ReadyToRun
{
    /// <summary>
    /// Provides a lazy-initialized implementation of <see cref="MethodEntryPointsSection"/> that is read from an
    /// input .NET executable file.
    /// </summary>
    public class SerializedMethodEntryPointsSection : MethodEntryPointsSection
    {
        private readonly BinaryStreamReader _reader;

        /// <summary>
        /// Reads a method entry points section from the provided input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        public SerializedMethodEntryPointsSection(ref BinaryStreamReader reader)
        {
            _reader = reader;
        }

        /// <inheritdoc />
        protected override IList<MethodEntryPoint> GetEntryPoints()
        {
            var result = base.GetEntryPoints();

            var array = new NativeArrayView(_reader);
            for (int i = 0; i < array.Count; i++)
            {
                if (array.TryGet(i, out var elementReader))
                    result.Add(ReadMethodEntryPoint(elementReader));
            }

            return result;
        }

        private static MethodEntryPoint ReadMethodEntryPoint(BinaryStreamReader elementReader)
        {
            uint header = NativeArrayView.DecodeUnsigned(ref elementReader);
            bool hasFixups = (header & 1) != 0;
            uint functionIndex = header >> 1;

            var entryPoint = new MethodEntryPoint(functionIndex);

            if (hasFixups)
                ReadFixups(entryPoint, elementReader);

            return entryPoint;
        }

        private static void ReadFixups(MethodEntryPoint entryPoint, BinaryStreamReader fixupsReader)
        {
            var nibbleReader = new NibbleReader(fixupsReader);

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
