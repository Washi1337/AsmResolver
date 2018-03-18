using System.Collections.Generic;
using System.Linq;

namespace AsmResolver.Net.Cil
{
    public class CilRawFatMethodBody : CilRawMethodBody
    {
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
        
        public bool HasSections
        {
            get { return (_header & 0x8) == 0x8; }
            set { _header = (ushort) ((_header & ~0x8) | (value ? 0x8 : 0)); }
        }

        public bool InitLocals
        {
            get { return (_header & 0x10) == 0x10; }
            set { _header = (ushort) ((_header & ~0x10) | (value ? 0x10 : 0)); }
        }

        public ushort MaxStack
        {
            get;
            set;
        }

        public uint LocalVarSigToken
        {
            get;
            set;
        }

        public IList<CilExtraSection> ExtraSections
        {
            get;
            private set;
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