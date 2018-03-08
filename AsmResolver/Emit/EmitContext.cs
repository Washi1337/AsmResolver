using System;

namespace AsmResolver.Emit
{
    public class EmitContext
    {
        public EmitContext(WindowsAssembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException("assembly");
            Assembly = assembly;
        }

        public WindowsAssembly Assembly
        {
            get;
            private set;
        }
    }
}
