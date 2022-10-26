using System;

namespace AsmResolver.DotNet.TestCases.Generics
{
    public class NonGenericType
    {
        public static void NonGenericMethodInNonGenericType()
        {
        }

        public static void GenericMethodInNonGenericType<U1, U2, U3>()
        {
        }

        public static void GenericMethodWithConstraints<T1, T2>()
            where T1 : IFoo
            where T2 : IFoo, IBar
        {

        }

        public static T GenericMethodWithReturnType<T>()
        {
            return default;
        }

    }

    public interface IFoo
    {
    }

    public interface IBar
    {
    }
}
