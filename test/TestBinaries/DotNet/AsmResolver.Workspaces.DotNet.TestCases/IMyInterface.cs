namespace AsmResolver.Workspaces.DotNet.TestCases
{
    public interface IMyInterface
    {
        void Explicit();

        bool Implicit();

        int Shadowed(int x);
        
        bool ExplicitP { get; set; }
        
        int ImplicitP { get; set; }
        
        float ShadowedP { get; set; }
        
    }
}
