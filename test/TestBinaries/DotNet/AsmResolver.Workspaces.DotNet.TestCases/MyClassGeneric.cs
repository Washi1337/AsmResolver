namespace AsmResolver.Workspaces.DotNet.TestCases
{
    public class MyClassGeneric : IMyInterfaceGeneric<bool,int,float>
    {
        /// <inheritdoc />
        void IMyInterfaceGeneric<bool,int,float>.Explicit()
        {
        }

        /// <inheritdoc />
        public virtual bool Implicit() => false;

        /// <inheritdoc />
        public virtual int Shadowed(int x) => 0;
        
        /// <inheritdoc />
        bool IMyInterfaceGeneric<bool,int,float>.ExplicitP { get; set; }
        
        /// <inheritdoc />
        public virtual int ImplicitP { get; set; } = 0;
        
        /// <inheritdoc />
        public virtual float ShadowedP { get; set; } = 0f;
    }
}
