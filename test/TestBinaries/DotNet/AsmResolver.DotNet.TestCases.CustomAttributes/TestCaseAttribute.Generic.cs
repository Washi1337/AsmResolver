using System;

namespace AsmResolver.DotNet.TestCases.CustomAttributes
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class TestCaseAttribute<T> : Attribute
    {
        public TestCaseAttribute()
        {
            Value = default;
        }

        public TestCaseAttribute(T value)
        {
            Value = value;
        }

        public T Value
        {
            get;
            set;
        }
    }
}
