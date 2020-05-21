using System;

namespace AsmResolver.DotNet.TestCases.CustomAttributes
{
    [TestCase]
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

        [TestCase(1)]
        public void FixedInt32Argument()
        {
        }
        
        [TestCase("String fixed arg")]
        public void FixedStringArgument()
        {
        }
        
        [TestCase(TestEnum.Value3)]
        public void FixedEnumArgument()
        {
        }
        
        [TestCase(typeof(string))]
        public void FixedTypeArgument()
        {
        }
        
        [TestCase(2, "Fixed arg", TestEnum.Value3)]
        public void FixedMultipleArguments()
        {
        }
        
        [TestCase(IntValue = 2)]
        public void NamedInt32Argument()
        {
        }
        
        [TestCase(StringValue = "String named arg")]
        public void NamedStringArgument()
        {
        }
        
        [TestCase(EnumValue = TestEnum.Value2)]
        public void NamedEnumArgument()
        {
        }
        
        [TestCase(TypeValue = typeof(int))]
        public void NamedTypeArgument()
        {
        }
    }
}