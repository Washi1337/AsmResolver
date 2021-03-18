namespace AsmResolver.Workspaces.DotNet.TestCases
{
    public abstract class MyClassGeneric<T1,T2> : IMyInterfaceGeneric<bool,int,float>
    {
        /// <inheritdoc />
        void IMyInterfaceGeneric<bool,int,float>.Explicit()
        {
        }

        public abstract T1 GenericMethod(T2 x);

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
