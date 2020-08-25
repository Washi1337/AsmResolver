using System;

namespace AsmResolver.DotNet.TestCases.CustomAttributes
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class TestCaseAttribute : Attribute
    {
        public TestCaseAttribute()
        {
        }

        public TestCaseAttribute(int value)
        {
            IntValue = value;
        }

        public TestCaseAttribute(string value)
        {
            StringValue = value;
        }

        public TestCaseAttribute(TestEnum enumValue)
        {
            EnumValue = enumValue;
        }

        public TestCaseAttribute(Type typeValue)
        {
            TypeValue = typeValue;
        }

        public TestCaseAttribute(int[] arrayValue)
        {
            ArrayValue = arrayValue;
        }

        public TestCaseAttribute(int intValue, string stringValue, TestEnum enumValue)
        {
            IntValue = intValue;
            StringValue = stringValue;
            EnumValue = enumValue;
        }

        public TestCaseAttribute(object objValue)
        {
            ObjectValue = objValue;
        }

        public int IntValue
        {
            get;
            set;
        }

        public string StringValue
        {
            get;
            set;
        }

        public TestEnum EnumValue
        {
            get;
            set;
        }

        public Type TypeValue
        {
            get;
            set;
        }

        public object ObjectValue
        {
            get;
            set;
        }

        public int[] ArrayValue
        {
            get;
            set;
        }
    }
}