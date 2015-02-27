using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsmResolver.Builder
{
    public class BuildingParameters
    {
        public BuildingParameters(BinaryStreamWriter writer)
        {
            Writer = writer;
        }

        public BinaryStreamWriter Writer
        {
            get;
            private set;
        }
    }
}
