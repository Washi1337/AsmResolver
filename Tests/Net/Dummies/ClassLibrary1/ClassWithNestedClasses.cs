using System;

namespace ClassLibrary1
{
    public class ClassWithNestedClasses
    {
        public class NestedClass
        {
            public string Test;

            public NestedClass(string test)
            {
                Test = test;
            }
        }

        public NestedClass MyField;

        public ClassWithNestedClasses()
        {
            MyField = new NestedClass("abc");
        }
        
        public string SomeMethod(int x)
        {
            return MyField.Test + x;
        }
    }
}