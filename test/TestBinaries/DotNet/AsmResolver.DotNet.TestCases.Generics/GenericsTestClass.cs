namespace AsmResolver.DotNet.TestCases.Generics
{
    public class GenericsTestClass
    {
        public GenericType<string, int, object> GenericField;
        
        public GenericType<string, int, object> TypeInstantiation()
        {
            return new GenericType<string, int, object>();
        }
        
        public void MethodInstantiationFromNonGenericType()
        {
            NonGenericType.GenericMethodInNonGenericType<float, double, string>();
        }

        public void MethodInstantiationFromGenericType()
        {
            GenericType<byte, ushort, uint>.GenericMethodInGenericType<sbyte, short, int>();
        }
    }
}