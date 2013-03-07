using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.PE.Writers
{
    internal interface ICalculationTask
    {
        uint NewSize { get; }
        void CalculateOffsetsAndSizes();
    }
}
