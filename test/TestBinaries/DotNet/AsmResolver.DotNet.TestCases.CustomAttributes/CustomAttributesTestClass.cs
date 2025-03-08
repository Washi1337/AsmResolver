using System;
using System.Collections.Generic;

// Disable warnings for unused members.
#pragma warning disable 67

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

        [TestCase((object) TestEnum.Value3)]
        public void FixedEnumAsObjectArgument()
        {
        }

        [TestCase(typeof(string))]
        public void FixedTypeArgument()
        {
        }

        [TestCase(typeof(KeyValuePair<string[], int[]>))]
        public void FixedComplexTypeArgument()
        {
        }

        [TestCase(typeof(TestEnum))]
        public void FixedLocalTypeArgument()
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

        [TestCase(ObjectValue = TestEnum.Value2)]
        public void NamedEnumAsObjectArgument()
        {
        }

        [TestCase(TypeValue = typeof(int))]
        public void NamedTypeArgument()
        {
        }

        [TestCase(ObjectValue = typeof(int))]
        public void NamedTypeAsObjectArgument()
        {
        }

        [TestCase(typeof(TestGenericType<object>))]
        public void GenericType()
        {
        }

        [TestCase(typeof(TestGenericType<object>[]))]
        public void GenericTypeArray()
        {
        }

        [TestCase((object) 123)]
        public void Int32PassedAsObject()
        {
        }

        [TestCase((object) typeof(int))]
        public void TypePassedAsObject()
        {
        }

        [TestCase(new int[] {1, 2, 3, 4})]
        public void FixedInt32ArrayArgument()
        {
        }

        [TestCase(default(int[]))]
        public void FixedInt32ArrayNullArgument()
        {
        }

        [TestCase(new int[0])]
        public void FixedInt32ArrayEmptyArgument()
        {
        }

        [TestCase((object) new int[] {1, 2, 3, 4})]
        public void FixedInt32ArrayAsObjectArgument()
        {
        }

        [TestCase((object) default(int[]))]
        public void FixedInt32ArrayAsObjectNullArgument()
        {
        }

        [TestCase((object) new int[0])]
        public void FixedInt32ArrayAsObjectEmptyArgument()
        {
        }

        [TestCase((Type) null)]
        public void FixedNullTypeArgument()
        {
        }

        [TestCase((object) typeof(int))]
        public void FixedTypeAsObjectArgument()
        {
        }

        [TestCase<int>(1)]
        public void FixedGenericInt32Argument()
        {
        }

        [TestCase<string>("Fixed string generic argument")]
        public void FixedGenericStringArgument()
        {
        }

        [TestCase<int[]>(new int[] {1,2,3,4})]
        public void FixedGenericInt32ArrayArgument()
        {
        }

        [TestCase<object>(new int[] {1,2,3,4})]
        public void FixedGenericInt32ArrayAsObjectArgument()
        {
        }

        [TestCase<Type>(typeof(int))]
        public void FixedGenericTypeArgument()
        {
        }

        [TestCase<Type>(null)]
        public void FixedGenericTypeNullArgument()
        {
        }

        [TestCase<int>(Value = 1)]
        public void NamedGenericInt32Argument()
        {
        }

        [TestCase<string>(Value = "Named string generic argument")]
        public void NamedGenericStringArgument()
        {
        }

        [TestCase<int[]>(Value = new int[] {1,2,3,4})]
        public void NamedGenericInt32ArrayArgument()
        {
        }

        [TestCase<object>(Value = new int[] {1,2,3,4})]
        public void NamedGenericInt32ArrayAsObjectArgument()
        {
        }

        [TestCase<Type>(Value = typeof(int))]
        public void NamedGenericTypeArgument()
        {
        }

        [TestCase<object>(Value = typeof(int))]
        public void NamedGenericTypeAsObjectArgument()
        {
        }

        [TestCase<Type>(Value = null)]
        public void NamedGenericTypeNullArgument()
        {
        }
    }
}
