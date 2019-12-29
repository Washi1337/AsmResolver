using System;

namespace AsmResolver.DotNet.TestCases.CustomAttributes
{
    [TestCase]
    [TestCase(1)]
    [TestCase("String fixed arg")]
    [TestCase(TestEnum.Value2)]
    [TestCase(2, "Fixed arg", TestEnum.Value3)]
    [TestCase(IntValue = 3, StringValue = "Named arg", EnumValue = TestEnum.Value2)]
    public class CustomAttributesTestClass
    {
        [TestCase]
        public event EventHandler TestEvent;
        
        [TestCase]
        public int TestField;
        
        [TestCase]
        public void TestMethod([TestCase] int testParameter)
        {
        }

        [TestCase]
        public int TestProperty
        {
            [TestCase]
            get;
            [TestCase]
            set;
        }
    }
}