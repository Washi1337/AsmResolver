namespace AsmResolver.DotNet.TestCases.Types
{
    public class InterfaceImplementation : IInterface1, IInterface2
    {
        public void Interface1Method()
        {
        }

        void IInterface2.Interface2Method()
        {
        }
    }
}