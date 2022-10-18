using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.IO;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.PE.DotNet.Cil
{
    /// <summary>
    /// Represents a CIL method body using the fat format.
    /// </summary>
    /// <remarks>
    /// The fat method body format is used when a CIL method body's code size is larger than 64 bytes, has  local
    /// variables, its max stack size is greater than 8, or uses extra sections (e.g. for storing exception handlers).
    ///
    /// This class does not do any encoding/decoding of the bytes that make up the actual CIL instructions, nor does
    /// it do any verification of the code.
    /// </remarks>
    public class CilRawFatMethodBody : CilRawMethodBody
    {
        private CilMethodBodyAttributes _attributes;

        /// <summary>
        /// Creates a new fat method body.
        /// </summary>
        /// <param name="attributes">The attributes associated to the method body.</param>
        /// <param name="maxStack">The maximum amount of values that can be pushed onto the stack.</param>
        /// <param name="localVarSigToken">The metadata token that defines the local variables for the method body.</param>
        /// <param name="code">The raw code of the method.</param>
        public CilRawFatMethodBody(
            CilMethodBodyAttributes attributes,
            ushort maxStack,
            MetadataToken localVarSigToken,
            IReadableSegment code)
            : base(code)
        {
            Attributes = attributes;
            MaxStack = maxStack;
            LocalVarSigToken = localVarSigToken;
        }

        /// <inheritdoc />
        public override bool IsFat => true;

        /// <summary>
        /// Gets or sets the attributes associated to the method body.
        /// </summary>
        /// <remarks>
        /// This property always has the <see cref="CilMethodBodyAttributes.Fat"/> flag set.
        /// </remarks>
        public CilMethodBodyAttributes Attributes
        {
            get => _attributes;
            set => _attributes = value | CilMethodBodyAttributes.Fat;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the method body stores extra sections.
        /// </summary>
        /// <remarks>
        /// This property does not automatically update when <see cref="ExtraSections"/> is changed.
        /// </remarks>
        public bool HasSections
        {
            get => (Attributes & CilMethodBodyAttributes.MoreSections) != 0;
            set => Attributes = (Attributes & ~CilMethodBodyAttributes.MoreSections)
                                | (value ? CilMethodBodyAttributes.MoreSections : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether all locals defined by this method body should be initialized
        /// to zero by the runtime upon starting execution of the method body.
        /// </summary>
        public bool InitLocals
        {
            get => (Attributes & CilMethodBodyAttributes.InitLocals) != 0;
            set => Attributes = (Attributes & ~CilMethodBodyAttributes.InitLocals)
                                | (value ? CilMethodBodyAttributes.InitLocals : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating the maximum amount of values that can be pushed onto the stack by the
        /// code stored in the method body.
        /// </summary>
        public ushort MaxStack
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the metadata token referencing a signature that defines all local variables in the method body.
        /// </summary>
        public MetadataToken LocalVarSigToken
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a collection of extra metadata sections that are appended to the method body.
        /// </summary>
        /// <remarks>
        /// These sections are used to encode any exception handler in the method body.
        /// </remarks>
        public IList<CilExtraSection> ExtraSections
        {
            get;
        } = new List<CilExtraSection>();

        /// <summary>
        /// Reads a raw method body from the given binary input stream using the fat method body format.
        /// </summary>
        /// <param name="errorListener">The object responsible for recording parser errors.</param>
        /// <param name="reader">The binary input stream to read from.</param>
        /// <returns>The raw method body.</returns>
        /// <exception cref="FormatException">Occurs when the method header indicates an method body that is not in the
        /// fat format.</exception>
        public new static CilRawFatMethodBody? FromReader(IErrorListener errorListener, ref BinaryStreamReader reader)
        {
            ulong fileOffset = reader.Offset;
            uint rva = reader.Rva;

            // Read flags.
            ushort header = reader.ReadUInt16();
            var flags = (CilMethodBodyAttributes) (header & 0xFFF);
            int headerSize = (header >> 12) * sizeof(uint);

            // Verify this is a fat method body.
            if ((flags & CilMethodBodyAttributes.Fat) != CilMethodBodyAttributes.Fat)
            {
                errorListener.BadImage("Invalid fat CIL method body header.");
                return null;
            }

            // Read remaining header.
            ushort maxStack = reader.ReadUInt16();
            uint codeSize = reader.ReadUInt32();
            uint localVarSigToken = reader.ReadUInt32();

            // Move to code.
            reader.Offset = fileOffset + (ulong) headerSize;

            // Verify code size.
            if (reader.Offset + codeSize > reader.StartOffset + reader.Length)
            {
                errorListener.BadImage("Invalid fat CIL method body code size.");
                return null;
            }

            // Read code.
            var code = reader.ReadSegment(codeSize);

            // Create body.
            var body = new CilRawFatMethodBody(flags, maxStack, localVarSigToken, code);
            body.UpdateOffsets(new RelocationParameters(fileOffset, rva));

            // Read any extra sections.
            if (body.HasSections)
            {
                reader.Align(4);

                CilExtraSection section;
                do
                {
                    section = CilExtraSection.FromReader(ref reader);
                    body.ExtraSections.Add(section);
                } while (section.HasMoreSections);
            }

            return body;
        }

        /// <inheritdoc />
        public override void UpdateOffsets(in RelocationParameters parameters)
        {
            base.UpdateOffsets(parameters);

            var current = parameters.WithAdvance(12);

            Code.UpdateOffsets(current);
            if (HasSections)
            {
                uint codeSize = Code.GetPhysicalSize();
                current.Advance(codeSize);
                current.Align(4);

                for (int i = 0; i < ExtraSections.Count; i++)
                {
                    ExtraSections[i].UpdateOffsets(current);
                    current.Advance(ExtraSections[i].GetPhysicalSize());
                }
            }
        }

        /// <inheritdoc />
        public override uint GetPhysicalSize()
        {
            uint length = 12 + Code.GetPhysicalSize();
            ulong endOffset = Offset + length;

            if (HasSections)
            {
                ulong sectionsOffset = endOffset.Align(4);
                length += (uint) (sectionsOffset - endOffset);

                for (int i = 0; i < ExtraSections.Count; i++)
                    length += ExtraSections[i].GetPhysicalSize();
            }

            return length;
        }

        /// <inheritdoc />
        public override void Write(IBinaryStreamWriter writer)
        {
            writer.WriteUInt16((ushort) ((ushort) _attributes | 0x3000));
            writer.WriteUInt16(MaxStack);
            writer.WriteUInt32(Code.GetPhysicalSize());
            writer.WriteUInt32(LocalVarSigToken.ToUInt32());
            Code.Write(writer);

            if (HasSections)
            {
                writer.Align(4);
                for (int i = 0; i < ExtraSections.Count; i++)
                    ExtraSections[i].Write(writer);
            }
        }
    }
}
