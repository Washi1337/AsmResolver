using System;

namespace ClassLibrary1
{
    public class ClassB
    {
        public string MyProperty
        {
            get { return "MyPropertyB"; }
        }
        
        public ClassA MyMethod()
        {
            return new ClassA();
        }

        public void Test(ClassA x)
        {
            Console.WriteLine(MyProperty + ": " + x.MyProperty);
        }
    }
}