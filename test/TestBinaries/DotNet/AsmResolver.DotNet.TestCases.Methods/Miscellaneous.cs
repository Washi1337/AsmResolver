using System;

namespace AsmResolver.DotNet.TestCases.Methods
{
    public class Miscellaneous
    {
        public class NestedClass
        {
            public int Property
            {
                get;
                set;
            }
        }
        
        public void NestedClassLocal()
        {
            NestedClass x = new NestedClass();
            x.Property = 123;
            Console.WriteLine(x.Property);
        }

        public void CallsToMethods()
        {
            MethodA();
            MethodB(123);
        }
        
        public void MethodA()
        {
        }

        public void MethodB(int x)
        {
        }

        public void OptionalParameter(int x = 123)
        {
        }
    }
}