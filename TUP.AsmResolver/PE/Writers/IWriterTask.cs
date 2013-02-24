using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.PE.Writers
{
    internal interface IWriterTask
    {
        PEWriter Writer { get; }
        void RunProcedure();
    }
}
