using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsmResolver
{
    public abstract class FileSegment
    {
        public long StartOffset
        {
            get;
            set;
        }

        public abstract uint GetPhysicalLength();

        public abstract void Write(WritingContext context);

        public static uint Align(uint value, uint align)
        {
            align--;
            return (value + align) & ~align;
        }
    }
}
