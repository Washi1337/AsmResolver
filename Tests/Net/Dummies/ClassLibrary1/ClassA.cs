using System;

namespace ClassLibrary1
{
    public class ClassA
    {
        public string MyProperty
        {
            get { return "MyPropertyA"; }
        }

        public ClassB MyMethod()
        {
            return new ClassB();
        }

        public void Test(ClassB x)
        {
            Console.WriteLine(MyProperty + ": " + x.MyProperty);
        }
    }
}