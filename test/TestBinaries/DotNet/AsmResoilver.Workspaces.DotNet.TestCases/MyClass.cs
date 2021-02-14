namespace AsmResoilver.Workspaces.DotNet.TestCases
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
    }
}
