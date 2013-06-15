using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUP.AsmResolver.PE.Writers
{
    public abstract class RebuildingTask
    {
        public RebuildingTask(PEConstructor constructor)
        {
            Constructor = constructor;
        }

        public PEConstructor Constructor { get; private set; }

        public abstract void RunProcedure(Workspace workspace);
    }
}
