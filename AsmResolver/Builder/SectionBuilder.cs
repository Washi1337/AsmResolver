using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Builder;

namespace AsmResolver.Builder
{
    public class SectionBuilder : FileSegmentBuilder
    {
        private readonly NetAssemblyBuilder _builder;

        public SectionBuilder(NetAssemblyBuilder builder, string name)
        {
            _builder = builder;
            Header = new ImageSectionHeader()
            {
                Name = name
            };
        }

        public ImageSectionHeader Header
        {
            get;
            private set;
        }

        public uint GetVirtualSize()
        {
            return base.GetPhysicalLength();
        }

        public override uint GetPhysicalLength()
        {
            return Align(base.GetPhysicalLength(), _builder.Assembly.NtHeaders.OptionalHeader.FileAlignment);
        }

        public override void Write(WritingContext context)
        {
            // TODO: more elegant way of creating buffer.
            context.Writer.Position += GetPhysicalLength();
            context.Writer.Position -= GetPhysicalLength();
            base.Write(context);
        }
    }
}
