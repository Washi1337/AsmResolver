using AsmResolver.IO;
using AsmResolver.Shims;

namespace AsmResolver.PE.Exceptions
{
    /// <summary>
    /// Encodes the effects a function has on the stack pointer, and where the nonvolatile
    /// registers are saved on the stack.
    /// </summary>
    public class X64UnwindInfo : SegmentBase, IUnwindInfo
    {
        private byte _firstByte;
        private byte _frameByte;

        /// <summary>
        /// Creates a new empty instance of the <see cref="X64UnwindInfo"/> class.
        /// </summary>
        public X64UnwindInfo()
        {
            Version = 1;
            UnwindCodes = ArrayShim.Empty<ushort>();
            ExceptionHandler = SegmentReference.Null;
            ExceptionHandlerData = SegmentReference.Null;
        }

        /// <summary>
        /// Gets or sets the version number of the unwind info..
        /// </summary>
        /// <remarks>
        /// Should be 1.
        /// </remarks>
        public byte Version
        {
            get => (byte) (_firstByte & 0b00000111);
            set => _firstByte = (byte) ((_firstByte & 0b11111000) | (value & 0b00000111));
        }

        /// <summary>
        /// Gets or sets the flags associated to the unwind information.
        /// </summary>
        public X64UnwindFlags Flags
        {
            get => (X64UnwindFlags) (_firstByte >> 3);
            set => _firstByte = (byte) ((_firstByte & 0b00000111) | ((int) value << 3));
        }

        /// <summary>
        /// Gets or sets a value indicating whether the function has an exception handler that should be called when
        /// looking for functions that need to examine exceptions.
        /// </summary>
        public bool IsExceptionHandler
        {
            get => (Flags & X64UnwindFlags.ExceptionHandler) != 0;
            set => Flags = (Flags & ~X64UnwindFlags.ExceptionHandler) | (value ? X64UnwindFlags.ExceptionHandler : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the function has a termination handler that should be called
        /// when unwinding an exception.
        /// </summary>
        public bool IsTerminationHandler
        {
            get => (Flags & X64UnwindFlags.TerminationHandler) != 0;
            set => Flags = (Flags & ~X64UnwindFlags.TerminationHandler) | (value ? X64UnwindFlags.TerminationHandler : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether this unwind info structure is not the primary one for the procedure.
        /// Instead, the chained unwind info entry is the contents of a previous RUNTIME_FUNCTION entry.
        /// </summary>
        public bool IsChained
        {
            get => (Flags & X64UnwindFlags.ChainedUnwindInfo) != 0;
            set => Flags = (Flags & ~X64UnwindFlags.ChainedUnwindInfo) | (value ? X64UnwindFlags.ChainedUnwindInfo : 0);
        }

        /// <summary>
        /// Gets or sets the length of the function prolog in bytes.
        /// </summary>
        public byte SizeOfProlog
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the number of the nonvolatile register used as the frame pointer, using the same encoding for the
        /// operation info field of UNWIND_CODE nodes.
        /// </summary>
        public byte FrameRegister
        {
            get => (byte) (_frameByte & 0b00001111);
            set => _frameByte = (byte) ((_frameByte & 0b11110000) | (value & 0b00001111));
        }

        /// <summary>
        /// If <see cref="FrameRegister"/> is nonzero, gets or sets the scaled offset from RSP that is applied to the
        /// FP register when it is established.
        /// </summary>
        public byte FrameRegisterOffset
        {
            get => (byte) (_frameByte >> 4);
            set => _frameByte = (byte) ((_frameByte & 0b00001111) | (value  << 4));
        }

        /// <summary>
        /// Gets or sets an array of items that explains the effect of the prolog on the nonvolatile registers and RSP.
        /// </summary>
        public ushort[] UnwindCodes
        {
            get;
            set;
        }

        /// <summary>
        /// When <see cref="IsExceptionHandler"/> or <see cref="IsTerminationHandler"/> is <c>true</c>, gets or sets
        /// the reference to the exception handler assigned to this unwind information.
        /// </summary>
        public ISegmentReference ExceptionHandler
        {
            get;
            set;
        }

        /// <summary>
        /// When <see cref="IsExceptionHandler"/> or <see cref="IsTerminationHandler"/> is <c>true</c>, gets or sets
        /// the reference to the exception handler data assigned to this unwind information.
        /// </summary>
        public ISegmentReference ExceptionHandlerData
        {
            get;
            set;
        }

        /// <summary>
        /// When <see cref="IsChained"/> is <c>true</c>, gets or sets the function that this unwind information is
        /// chained with.
        /// </summary>
        public X64RuntimeFunction? ChainedFunction
        {
            get;
            set;
        }

        /// <summary>
        /// Reads unwind information from the provided input stream.
        /// </summary>
        /// <param name="context">The reader context.</param>
        /// <param name="reader">The input stream.</param>
        /// <returns>The read unwind information.</returns>
        public static X64UnwindInfo FromReader(PEReaderContext context, ref BinaryStreamReader reader)
        {
            var result = new X64UnwindInfo();
            result.UpdateOffsets(context.GetRelocation(reader.Offset, reader.Rva));

            result._firstByte = reader.ReadByte();
            result.SizeOfProlog = reader.ReadByte();

            byte count = reader.ReadByte();
            result._frameByte = reader.ReadByte();

            ushort[] codes = new ushort[count];
            for (int i = 0; i < count; i++)
                codes[i] = reader.ReadUInt16();

            result.UnwindCodes = codes;

            if (result.IsExceptionHandler || result.IsTerminationHandler)
            {
                result.ExceptionHandler = context.File.GetReferenceToRva(reader.ReadUInt32());
                result.ExceptionHandlerData = context.File.GetReferenceToRva(reader.Rva);
            }
            else if (result.IsChained)
            {
                result.ChainedFunction = X64RuntimeFunction.FromReader(context, ref reader);
            }

            return result;
        }

        /// <inheritdoc />
        public override uint GetPhysicalSize()
        {
            uint size = sizeof(byte) // flags:version
                        + sizeof(byte) // size of prolog
                        + sizeof(byte) // count
                        + sizeof(byte) // frame register:frame register offset
                        + (uint) UnwindCodes.Length * sizeof(ushort); // unwind codes.

            if (IsExceptionHandler)
            {
                size += sizeof(uint); // EH address
            }
            else if (IsChained)
            {
                size += sizeof(uint) // Function start
                        + sizeof(uint) // Function end
                        + sizeof(uint); // Unwind info address.
            }

            return size;
        }

        /// <inheritdoc />
        public override void Write(BinaryStreamWriter writer)
        {
            writer.WriteByte(_firstByte);
            writer.WriteByte(SizeOfProlog);
            writer.WriteByte((byte) UnwindCodes.Length);
            writer.WriteByte(_frameByte);

            foreach (ushort code in UnwindCodes)
                writer.WriteUInt16(code);

            if (IsExceptionHandler || IsTerminationHandler)
            {
                writer.WriteUInt32(ExceptionHandler.Rva);

                // TODO: include custom EH data.
            }
            else if (IsChained && ChainedFunction is not null)
            {
                ChainedFunction.Write(writer);
            }
        }
    }
}
