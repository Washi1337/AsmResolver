using System;
using System.Collections.Generic;
using AsmResolver.Collections;
using AsmResolver.IO;
using AsmResolver.PE.Relocations;

namespace AsmResolver.PE;

internal static class Extensions
{
    public static uint GetFlags(this uint self, int index, uint mask) => (self & mask) >> index;

    public static uint SetFlags(this uint self, int index, uint mask, uint value) => (self & ~mask) | ((value << index) & mask);

    public static IEnumerable<BaseRelocation> CreateBaseRelocations(this ReferenceTable self)
    {
        if (!self.IsVaTable)
            yield break;

        (int pointerSize, var type) = self.Is32BitTable
            ? (sizeof(uint), RelocationType.HighLow)
            : (sizeof(ulong), RelocationType.Dir64);

        for (int i = 0; i < self.Count; i++)
            yield return new BaseRelocation(type, self.ToReference(i * pointerSize));
    }

    extension(ref BinaryStreamReader self)
    {
        public ReferenceTable ReadReferenceTable(
            PEReaderContext context,
            ReferenceTableAttributes attributes,
            int? count = null
        )
        {
            return self.ReadReferenceTable(context, new ReferenceTable(attributes), count);
        }

        public ReferenceTable ReadReferenceTable(
            PEReaderContext context,
            ReferenceTable result,
            int? count = null
        )
        {
            if (result.IsZeroTerminated)
            {
                if (count is not null)
                    throw new ArgumentException("Attempted to read a zero-terminated reference table with a count.");

                return self.ReadZeroTerminatedTable(context, result);
            }

            if (count.HasValue)
                return self.ReadSizedTable(context, result, count.Value);

            throw new ArgumentException("Attempted to read a non-zero-terminated reference table with no count.");
        }

        private ReferenceTable ReadZeroTerminatedTable(PEReaderContext context, ReferenceTable result)
        {
            System.Diagnostics.Debug.Assert(result.IsZeroTerminated);

            result.UpdateOffsets(context.GetRelocation(self.Offset, self.Rva));
            bool is32Bit = result.Is32BitTable;

            while (true)
            {
                if (!self.CanRead((uint) (is32Bit ? sizeof(uint) : sizeof(ulong))))
                {
                    context.BadImage("Reference table does not end with a zero entry.");
                    break;
                }

                var reference = self.ReadReference(context, result);
                if (result.IsZeroTerminated && reference == SegmentReference.Null)
                    break;

                result.Add(reference);
            }

            return result;
        }

        private ReferenceTable ReadSizedTable(PEReaderContext context, ReferenceTable result, int count)
        {
            System.Diagnostics.Debug.Assert(!result.IsZeroTerminated);

            result.UpdateOffsets(context.GetRelocation(self.Offset, self.Rva));
            bool is32Bit = result.Is32BitTable;

            for (int i = 0; i < count; i++)
            {
                if (!self.CanRead((uint) (is32Bit ? sizeof(uint) : sizeof(ulong))))
                {
                    context.BadImage("Reference table is too large");
                    break;
                }

                result.Add(self.ReadReference(context, result));
            }

            return result;
        }

        private ISegmentReference ReadReference(PEReaderContext context, ReferenceTable result)
        {
            ulong rawValue = self.ReadNativeInt(result.Is32BitTable);
            if (rawValue == 0)
                return SegmentReference.Null;

            uint rva = result.ReferenceType switch
            {
                ReferenceTableAttributes.Offset => context.File.FileOffsetToRva(rawValue),
                ReferenceTableAttributes.Rva => (uint) rawValue,
                ReferenceTableAttributes.Va => (uint) (rawValue - context.File.OptionalHeader.ImageBase),
                _ => throw new ArgumentOutOfRangeException()
            };

            return context.File.GetReferenceToRva(rva);
        }
    }
}
