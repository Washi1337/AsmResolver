using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.PE.Writers
{
    internal class PEStructureFixer : IWriterTask 
    {
        internal PEStructureFixer(PEWriter writer)
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
            // TODO: Fix all offsets and sizes to allow adding/removing members.

        }
    }
}
