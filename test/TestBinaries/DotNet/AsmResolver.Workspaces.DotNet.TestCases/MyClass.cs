namespace AsmResolver.Workspaces.DotNet.TestCases
{
    public class MyClass : IMyInterface
    {
        /// <inheritdoc />
        void IMyInterface.Explicit()
        {
        }

        /// <inheritdoc />
        public virtual bool Implicit() => false;

        /// <inheritdoc />
        public virtual int Shadowed(int x) => 0;
        
        /// <inheritdoc />
        bool IMyInterface.ExplicitP { get; set; }
        
        /// <inheritdoc />
        public virtual int ImplicitP { get; set; } = 0;
        
        /// <inheritdoc />
        public virtual float ShadowedP { get; set; } = 0f;
    }
}
