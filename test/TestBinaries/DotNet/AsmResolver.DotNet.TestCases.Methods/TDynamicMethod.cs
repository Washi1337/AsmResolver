using System;
using System.Reflection;
using System.Reflection.Emit;

namespace AsmResolver.DotNet.TestCases.Methods
{
    public class TDynamicMethod
    {
        public static DynamicMethod GenerateDynamicMethod()
        {
            Type[] helloArgs = {typeof(string), typeof(int)};

            // Create a dynamic method with the name "Hello", a return type
            // of Integer, and two parameters whose types are specified by
            // the array helloArgs. Create the method in the module that
            // defines the String class.
            DynamicMethod hello = new DynamicMethod("Hello",
                typeof(int),
                helloArgs,
                typeof(string).Module);

            // Create an array that specifies the parameter types of the
            // overload of Console.WriteLine to be used in Hello.
            Type[] writeStringArgs = {typeof(string)};
            // Get the overload of Console.WriteLine that has one
            // String parameter.
            MethodInfo writeString = typeof(Console).GetMethod("WriteLine",
                writeStringArgs);

            // Get an ILGenerator and emit a body for the dynamic method,
            // using a stream size larger than the IL that will be
            // emitted.
            ILGenerator il = hello.GetILGenerator(256);
            // Load the first argument, which is a string, onto the stack.
            il.Emit(OpCodes.Ldarg_0);
            // Call the overload of Console.WriteLine that prints a string.
            il.EmitCall(OpCodes.Call, writeString, null);
            // The Hello method returns the value of the second argument;
            // to do this, load the onto the stack and return.
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ret);

            // Add parameter information to the dynamic method. (This is not
            // necessary, but can be useful for debugging.) For each parameter,
            // identified by position, supply the parameter attributes and a
            // parameter name.
            hello.DefineParameter(1, ParameterAttributes.In, "message");
            hello.DefineParameter(2, ParameterAttributes.In, "valueToReturn");
            return hello;

        }
    }
}