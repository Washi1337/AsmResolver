using System;

namespace AsmResolver.DotNet.TestCases.Methods
{
    public class MultipleMethods
    {
        public MultipleMethods()
        {
        }
        
        public static void VoidParameterlessMethod()
        {
        }
        
        public static int IntParameterlessMethod()
        {
            return default;
        }
        
        public static MultipleMethods TypeDefOrRefParameterlessMethod()
        {
            return default;
        }

        public static void SingleParameterMethod(int intParameter)
        {
        }

        public static void MultipleParameterMethod(int intParameter, string stringParameter, MultipleMethods typeDefOrRefParameter)
        {
        }
    }
}