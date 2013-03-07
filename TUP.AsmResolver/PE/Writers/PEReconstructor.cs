using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.PE.Writers
{
    internal class PEReconstructor : IWriterTask 
    {
        internal PEReconstructor(PEWriter writer)
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

            foreach (IWriterTask task in Writer.Tasks)
                if (task is ICalculationTask && !(task is PEHeaderWriter))
                    ((ICalculationTask)task).CalculateOffsetsAndSizes();

            ((ICalculationTask)Writer.GetTask(typeof(PEHeaderWriter))).CalculateOffsetsAndSizes();

            foreach (IWriterTask task in Writer.Tasks)
                if (task is IReconstructionTask)
                    ((IReconstructionTask)task).Reconstruct();
        }
    }
}
