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
        
        public static void IntParameterlessMethod()
        {
        }
        
        public static MultipleMethods TypeDefOrRefParameterlessMethod()
        {
            return default;
        }

        public static void SingleParameterMethod(int intParameter)
        {
        }

        public static void MultipleParameterMethod(int intParameter, string stringParameter, MulticastDelegate typeDefOrRefParameter)
        {
        }
    }
}