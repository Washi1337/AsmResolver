namespace AsmResoilver.Workspaces.DotNet.TestCases
{
    public interface IMyInterface
    {
        void Explicit();

        bool Implicit();

        int Shadowed(int x);
    }
}
