namespace AsmResolver.DotNet.TestCases.Generics
{
    public class NonGenericType
    {
        public static void NonGenericMethodInNonGenericType()
        {
        }
        
        public static void GenericMethodInNonGenericType<U1, U2, U3>()
        {
        }
    }
}