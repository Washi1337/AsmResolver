using System.Threading;

namespace AsmResolver.DotNet.TestCases.Generics
{
    public class GenericsTestClass
    {
        public GenericType<string, int, object> GenericField;
        private string _field;
        
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

        public void MethodInstantiationFromExternalType()
        {
            // https://github.com/Washi1337/AsmResolver/issues/43
            
            Interlocked.CompareExchange(ref _field, _field, null);
        }
    }
}