using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsmResolver.Builder
{
    public abstract class WindowsAssemblyBuilder : FileSegmentBuilder
    {
        protected WindowsAssemblyBuilder(WindowsAssembly assembly, BuildingParameters parameters)
        {
            if (assembly == null)
                throw new ArgumentNullException("assembly");
            if (parameters == null)
                throw new ArgumentNullException("parameters");
            Assembly = assembly;
            Parameters = parameters;
        }

        public WindowsAssembly Assembly
        {
            get;
            private set;
        }

        public BuildingParameters Parameters
        {
            get;
            private set;
        }
        
    }
}
