using System;

namespace AsmResolver.Emit
{
    public class EmitContext
    {
        public EmitContext(WindowsAssemblyBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException("builder");
            this.Builder = builder;
        }

        public WindowsAssemblyBuilder Builder
        {
            get;
            private set;
        }
    }
}
