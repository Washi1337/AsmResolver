using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsmResolver.Builder
{
    public class BuildingContext 
    {
        public BuildingContext(WindowsAssemblyBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException("builder");
            Builder = builder;
        }

        public WindowsAssemblyBuilder Builder
        {
            get;
            private set;
        }

        public WindowsAssembly Assembly
        {
            get { return Builder.Assembly; }
        }

        public BuildingParameters Parameters
        {
            get { return Builder.Parameters; }
        }
    }
}
