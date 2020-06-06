using System;
using System.Reflection;
using System.Reflection.Emit;

namespace AsmResolver.DotNet.TestCases.Methods
{
    public class TDynamicMethod
    {
        public static DynamicMethod GenerateDynamicMethod()
        {
            Type[] helloArgs = {typeof(int), typeof(int)};

            DynamicMethod hello = new DynamicMethod("Test",
                typeof(int),
                helloArgs,
                typeof(TDynamicMethod).Module);

            
            ILGenerator adderIL = hello.GetILGenerator();
            Type overflow = typeof(OverflowException);
            ConstructorInfo exCtorInfo = overflow.GetConstructor(
                new Type[]
                    {typeof(string)});
            MethodInfo exToStrMI = overflow.GetMethod("ToString");
            MethodInfo writeLineMI = typeof(Console).GetMethod("WriteLine",
                new Type[]
                {
                    typeof(string),
                    typeof(string)
                });

            LocalBuilder tmp1 = adderIL.DeclareLocal(typeof(int));
            LocalBuilder tmp2 = adderIL.DeclareLocal(overflow);

            Label failed = adderIL.DefineLabel();
            Label endOfMthd = adderIL.DefineLabel();

            Label exBlock = adderIL.BeginExceptionBlock();

            adderIL.Emit(OpCodes.Ldarg_0);
            adderIL.Emit(OpCodes.Ldc_I4_S, 100);
            adderIL.Emit(OpCodes.Bgt_S, failed);

            adderIL.Emit(OpCodes.Ldarg_1);
            adderIL.Emit(OpCodes.Ldc_I4_S, 100);
            adderIL.Emit(OpCodes.Bgt_S, failed);

            adderIL.Emit(OpCodes.Ldarg_0);
            adderIL.Emit(OpCodes.Ldarg_1);
            adderIL.Emit(OpCodes.Add_Ovf_Un);

            adderIL.Emit(OpCodes.Stloc_S, tmp1);
            adderIL.Emit(OpCodes.Br_S, endOfMthd);

            adderIL.MarkLabel(failed);
            adderIL.Emit(OpCodes.Ldstr, "Cannot accept values over 100 for add.");
            adderIL.Emit(OpCodes.Newobj, exCtorInfo);



            adderIL.Emit(OpCodes.Stloc_S, tmp2);
            adderIL.Emit(OpCodes.Ldloc_S, tmp2);

            adderIL.ThrowException(overflow);
            adderIL.BeginCatchBlock(overflow);

            adderIL.Emit(OpCodes.Stloc_S, tmp2);
            adderIL.Emit(OpCodes.Ldstr, "Caught {0}");

            adderIL.Emit(OpCodes.Ldloc_S, tmp2);
            adderIL.EmitCall(OpCodes.Callvirt, exToStrMI, null);

            adderIL.EmitCall(OpCodes.Call, writeLineMI, null);

            adderIL.Emit(OpCodes.Ldc_I4_M1);
            adderIL.Emit(OpCodes.Stloc_S, tmp1);


            adderIL.EndExceptionBlock();

            adderIL.MarkLabel(endOfMthd);
            adderIL.Emit(OpCodes.Ldloc_S, tmp1);
            adderIL.Emit(OpCodes.Ret);
            return hello;
        }
    }
}