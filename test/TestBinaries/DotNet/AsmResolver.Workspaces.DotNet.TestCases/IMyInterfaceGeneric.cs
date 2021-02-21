namespace AsmResolver.Workspaces.DotNet.TestCases
{
    public interface IMyInterfaceGeneric<T1,T2,T3>
    {
        void Explicit();

        T1 Implicit();

        T2 Shadowed(T2 x);
        
        T1 ExplicitP { get; set; }
        
        T2 ImplicitP { get; set; }
        
        T3 ShadowedP { get; set; }
        
    }
}
