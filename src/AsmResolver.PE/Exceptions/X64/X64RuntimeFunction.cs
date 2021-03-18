using System;

namespace AsmResolver.PE.Exceptions.X64
{
    /// <summary>
    /// Represents a single runtime function in a Structured Exception Handler (SEH) table of a portable executable
    /// targeting the x64 (AMD64) architecture.
    /// </summary>
    public class X64RuntimeFunction : IRuntimeFunction, IWritable
    {
        /// <summary>
        /// Get th
        /// </summary>
        public const int EntrySize = sizeof(uint) * 3;

        /// <summary>
        /// Creates a new instance of the <see cref="X64RuntimeFunction"/> class.
        /// </summary>
        /// <param name="begin">The reference to the beginning of the function. </param>
        /// <param name="end">The reference to the end of the function.</param>
        /// <param name="unwindInfo">The unwind information associated to the function.</param>
        public X64RuntimeFunction(ISegmentReference begin, ISegmentReference end, X64UnwindInfo unwindInfo)
        {
            Begin = begin;
            End = end;
            UnwindInfo = unwindInfo;
        }

        /// <inheritdoc />
        public ISegmentReference Begin
        {
            get;
            set;
        }

        /// <inheritdoc />
        public ISegmentReference End
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the unwind information associated to the function.
        /// </summary>
        public X64UnwindInfo UnwindInfo
        {
            get;
            set;
        }

        /// <summary>
        /// Reads a single <see cref="X64RuntimeFunction"/> from the provided input stream.
        /// </summary>
        /// <param name="context">The reader context.</param>
        /// <param name="reader">The input stream.</param>
        /// <returns>The read function entry.</returns>
        public static X64RuntimeFunction FromReader(PEReaderContext context, IBinaryStreamReader reader)
        {
            uint begin = reader.ReadUInt32();
            uint end = reader.ReadUInt32();
            uint unwindInfo = reader.ReadUInt32(); // TODO: interpret unwindInfo.

            return new X64RuntimeFunction(
                context.File.GetReferenceToRva(begin),
                context.File.GetReferenceToRva(end),
                new X64UnwindInfo());
        }

        /// <inheritdoc />
        public uint GetPhysicalSize() => EntrySize;

        /// <inheritdoc />
        public void Write(IBinaryStreamWriter writer)
        {
            writer.WriteUInt32(Begin?.Rva ?? 0);
            writer.WriteUInt32(End?.Rva ?? 0);
            writer.WriteUInt32(UnwindInfo?.Rva ?? 0);
        }
    }
}
