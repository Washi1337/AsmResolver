using System;

namespace AsmResolver.DotNet.TestCases.CustomAttributes
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class TestCase2Attribute : Attribute
    {
        public TestCase2Attribute(Type type)
        {
            Type = type;
        }

        public Type Type { get; set; }

    }
}
