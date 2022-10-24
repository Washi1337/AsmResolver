using System;

namespace AsmResolver.DotNet.TestCases.Generics
{
    public class GenericType<T1, T2, T3>
    {
        public static void NonGenericMethodInGenericType()
        {
        }

        public static void GenericMethodInGenericType<U1, U2, U3>()
        {
        }

        public static T3 NonGenericMethodWithReturnType()
        {
            return default;
        }
    }
}
