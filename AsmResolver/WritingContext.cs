using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Builder;

namespace AsmResolver
{
    public class WritingContext
    {
        public WritingContext(WindowsAssembly assembly, IBinaryStreamWriter writer, BuildingContext buildingContext)
        {
            Assembly = assembly;
            Writer = writer;
            BuildingContext = buildingContext;
        }

        public WindowsAssembly Assembly
        {
            get;
            private set;
        }

        public BuildingContext BuildingContext
        {
            get;
            private set;
        }

        public IBinaryStreamWriter Writer
        {
            get;
            private set;
        }

    }
}
