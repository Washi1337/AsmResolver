using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsmResolver.Builder
{
    public class BuildingParameters
    {
        public BuildingParameters(IBinaryStreamWriter writer)
        {
            Writer = writer;
        }

        public IBinaryStreamWriter Writer
        {
            get;
            private set;
        }
    }
}
