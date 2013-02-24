using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.PE.Writers
{
    internal class SectionWriter : IWriterTask 
    {
        internal SectionWriter(PEWriter writer)
        {
            Writer = writer;
        }

        public PEWriter Writer
        {
            get;
            private set;
        }

        public void RunProcedure()
        {
            foreach (Section section in Writer.OriginalAssembly.NTHeader.Sections)
            {
                Writer.MoveToOffset(section.RawOffset);
                Writer.BinWriter.Write(section.GetBytes());
            }
                
        }
    }
}
