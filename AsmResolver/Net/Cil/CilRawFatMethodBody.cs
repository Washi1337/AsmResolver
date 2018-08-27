using System.Collections.Generic;
using System.Linq;

namespace AsmResolver.Net.Cil
{
    /// <summary>
    /// Represents a raw CIL method body that is using the fat format, including various customizable properties such as
    /// max stack, variables, and exception handlers. 
    /// </summary>
    public class CilRawFatMethodBody : CilRawMethodBody
    {
        /// <summary>
        /// Reads a raw fat CIL method body from the given input stream.
        /// </summary>
        /// <param name="reader">The input stream to read from.</param>
        /// <returns>The raw fat CIL method body.</returns>
        public new static CilRawFatMethodBody FromReader(IBinaryStreamReader reader)
        {
            var body = new CilRawFatMethodBody
            {
                _header = reader.ReadUInt16(),
                MaxStack = reader.ReadUInt16()
            };

            int codeSize = reader.ReadInt32();
            body.LocalVarSigToken = reader.ReadUInt32();
            
            body.Code = reader.ReadBytes(codeSize);

            if (body.HasSections)
            {
                reader.Align(4);
                
                CilExtraSection section;
                do
                {
                    section = CilExtraSection.FromReader(reader);
                    body.ExtraSections.Add(section);
                } while (section.HasMoreSections);
            }
            
            return body;
        }
        
        private ushort _header = 0x3003;
        
        public CilRawFatMethodBody()
        {
            ExtraSections = new List<CilExtraSection>();
        }
        
        /// <summary>
        /// Gets or sets a value indicating whether the method contains extra sections after the CIL code.
        /// </summary>
        public bool HasSections
        {
            get { return (_header & 0x8) == 0x8; }
            set { _header = (ushort) ((_header & ~0x8) | (value ? 0x8 : 0)); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the runtime should initialize the local variables with their default
        /// values.
        /// </summary>
        public bool InitLocals
        {
            get { return (_header & 0x10) == 0x10; }
            set { _header = (ushort) ((_header & ~0x10) | (value ? 0x10 : 0)); }
        }

        /// <summary>
        /// Gets or sets the maximum amount of values that can be stored onto the stack.
        /// </summary>
        public ushort MaxStack
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the metadata token referencing the standalone signature containing the local variables.
        /// </summary>
        public uint LocalVarSigToken
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a collection of sections that appear after the CIL method body.
        /// </summary>
        public IList<CilExtraSection> ExtraSections
        {
            get;
        }
        
        public override uint GetPhysicalLength()
        {
            uint length = (uint) (12 + Code.Length);
            uint endOffset = (uint) (StartOffset + length);
            
            uint sectionsOffset = Align(endOffset, 4);
            length += sectionsOffset - endOffset;

            length += (uint) ExtraSections.Sum(x => x.GetPhysicalLength());
            
            return length;
        }

        public override void Write(WritingContext context)
        {
            var writer = context.Writer;
            writer.WriteUInt16(_header);
            writer.WriteUInt16(MaxStack);
            writer.WriteInt32(Code.Length);
            writer.WriteUInt32(LocalVarSigToken);
            writer.WriteBytes(Code);

            if (HasSections)
            {
                writer.Align(4);
                foreach (var section in ExtraSections)
                    section.Write(context);
            }
        }
    }
}