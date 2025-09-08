using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using AsmResolver.Collections;
using AsmResolver.Shims;

namespace AsmResolver.DotNet
{
    internal static class ReflectionHacks
    {
#if !NET8_0_OR_GREATER
        private static readonly PropertyInfo? IsFunctionPointerProp = typeof(Type).GetProperty("IsFunctionPointer");
        private static readonly PropertyInfo? IsUnmanagedFunctionPointerProp = typeof(Type).GetProperty("IsUnmanagedFunctionPointer");
        private static readonly MethodInfo? GetFunctionPointerReturnTypeMethod = typeof(Type).GetMethod("GetFunctionPointerReturnType", ArrayShim.Empty<Type>());
        private static readonly MethodInfo? GetFunctionPointerCallingConventionsMethod = typeof(Type).GetMethod("GetFunctionPointerCallingConventions", ArrayShim.Empty<Type>());
        private static readonly MethodInfo? GetFunctionPointerParameterTypesMethod = typeof(Type).GetMethod("GetFunctionPointerParameterTypes", ArrayShim.Empty<Type>());
#endif
#if NETSTANDARD2_0
        private static readonly MethodInfo? GetHINSTANCEMethod = typeof(Marshal).GetMethod("GetHINSTANCE", new[] { typeof(Module) });
#endif

        internal static bool GetIsFunctionPointer(Type t)
        {
#if NET8_0_OR_GREATER
            return t.IsFunctionPointer;
#else
            return IsFunctionPointerProp != null && (bool)IsFunctionPointerProp.GetValue(t, ArrayShim.Empty<object>())!;
#endif
        }

        internal static bool GetIsUnmanagedFunctionPointer(Type t)
        {
#if NET8_0_OR_GREATER
            return t.IsUnmanagedFunctionPointer;
#else
            // can only be called if the type was already verified to be a function pointer
            // therefore the PropertyInfo is not null
            return (bool)IsUnmanagedFunctionPointerProp!.GetValue(t, ArrayShim.Empty<object>())!;
#endif
        }

        internal static Type GetFunctionPointerReturnType(Type t)
        {
#if NET8_0_OR_GREATER
            return t.GetFunctionPointerReturnType();
#else
            // can only be called if the type was already verified to be a function pointer
            // therefore the MethodInfo is not null
            return (Type)GetFunctionPointerReturnTypeMethod!.Invoke(t, null)!;
#endif
        }

        internal static Type[] GetFunctionPointerCallingConventions(Type t)
        {
#if NET8_0_OR_GREATER
            return t.GetFunctionPointerCallingConventions();
#else
            // can only be called if the type was already verified to be a function pointer
            // therefore the MethodInfo is not null
            return (Type[])GetFunctionPointerCallingConventionsMethod!.Invoke(t, null)!;
#endif
        }

        internal static Type[] GetFunctionPointerParameterTypes(Type t)
        {
#if NET8_0_OR_GREATER
            return t.GetFunctionPointerParameterTypes();
#else
            // can only be called if the type was already verified to be a function pointer
            // therefore the MethodInfo is not null
            return (Type[])GetFunctionPointerParameterTypesMethod!.Invoke(t, null)!;
#endif
        }

#if NET8_0_OR_GREATER
        [UnconditionalSuppressMessage("SingleFile", "IL3002", Justification = "Callers are explicitly checking for '-1'.")]
#endif
        internal static bool TryGetHINSTANCE(Module m, out nint hinstance)
        {
#if !NETSTANDARD2_0
            hinstance = Marshal.GetHINSTANCE(m);
            return true;
#else
            if (GetHINSTANCEMethod == null)
            {
                hinstance = default;
                return false;
            }
            hinstance = (nint)GetHINSTANCEMethod.Invoke(null, new object[] { m });
            return true;
#endif
        }
    }
}
