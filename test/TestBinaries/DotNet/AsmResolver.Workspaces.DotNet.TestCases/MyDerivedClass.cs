namespace AsmResolver.Workspaces.DotNet.TestCases
{
    public class MyDerivedClass : MyClass
    {
        /// <inheritdoc />
        public override bool Implicit() => true;

        public new virtual int Shadowed(int x) => 1;
    }
}
