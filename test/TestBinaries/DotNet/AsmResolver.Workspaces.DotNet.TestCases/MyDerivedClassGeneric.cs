namespace AsmResolver.Workspaces.DotNet.TestCases
{
    public class MyDerivedClassGeneric : MyClassGeneric<int,float>
    {
        public override int GenericMethod(float x)
        {
            return 1;
        }

        /// <inheritdoc />
        public override bool Implicit() => true;

        public new virtual int Shadowed(int x) => 1;
        
        /// <inheritdoc />
        public override int ImplicitP { get; set; } = 1;
        
        public new float ShadowedP { get; set; } = 1f;
    }
}
