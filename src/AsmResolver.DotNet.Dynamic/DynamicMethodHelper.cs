using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Serialized;
using AsmResolver.DotNet.Signatures;
using AsmResolver.IO;
using AsmResolver.PE.DotNet.Cil;

namespace AsmResolver.DotNet.Dynamic
{
    internal static class DynamicMethodHelper
    {
        public static void ReadLocalVariables(CilMethodBody methodBody, MethodDefinition method, byte[] localSig)
        {
            if (!(method.Module is SerializedModuleDefinition module))
                throw new ArgumentException("Method body should reference a serialized module.");

            var reader = ByteArrayDataSource.CreateReader(localSig);
            if (CallingConventionSignature.FromReader(
                new BlobReadContext(module.ReaderContext),
                ref reader) is not LocalVariablesSignature localsSignature)
            {
                throw new ArgumentException("Invalid local variables signature.");
            }

            for (int i = 0; i < localsSignature.VariableTypes.Count; i++)
                methodBody.LocalVariables.Add(new CilLocalVariable(localsSignature.VariableTypes[i]));
        }

        public static void ReadReflectionExceptionHandlers(CilMethodBody methodBody,
            IList<object>? ehInfos, byte[] ehHeader, ReferenceImporter importer)
        {
            //Sample needed!
            if (ehHeader is { Length: > 4 })
                throw new NotImplementedException("Exception handlers from ehHeader not supported yet.");

            if (ehInfos is { Count: > 0 })
            {
                foreach (var ehInfo in ehInfos)
                    InterpretEHInfo(methodBody, importer, ehInfo);
            }
        }

        private static void InterpretEHInfo(CilMethodBody methodBody, ReferenceImporter importer, object ehInfo)
        {
            for (int i = 0; i < FieldReader.ReadField<int>(ehInfo, "m_currentCatch"); i++)
            {
                // Get ExceptionHandlerInfo Field Values
                var endFinally = FieldReader.ReadField<int>(ehInfo, "m_endFinally");
                var instructions = methodBody.Instructions;

                var endFinallyLabel = endFinally >= 0
                    ? instructions.GetByOffset(endFinally)?.CreateLabel() ?? new CilOffsetLabel(endFinally)
                    : null;

                int tryStart = FieldReader.ReadField<int>(ehInfo, "m_startAddr");
                int tryEnd = FieldReader.ReadField<int>(ehInfo, "m_endAddr");
                int handlerStart = FieldReader.ReadField<int[]>(ehInfo, "m_catchAddr")![i];
                int handlerEnd = FieldReader.ReadField<int[]>(ehInfo, "m_catchEndAddr")![i];
                var exceptionType = FieldReader.ReadField<Type?[]>(ehInfo, "m_catchClass")![i];
                var handlerType = (CilExceptionHandlerType)FieldReader.ReadField<int[]>(ehInfo, "m_type")![i];

                var endTryLabel = instructions.GetByOffset(tryEnd)?.CreateLabel() ?? new CilOffsetLabel(tryEnd);

                // Create the handler
                var handler = new CilExceptionHandler
                {
                    HandlerType = handlerType,
                    TryStart = instructions.GetLabel(tryStart),
                    TryEnd = handlerType == CilExceptionHandlerType.Finally ? endFinallyLabel : endTryLabel,
                    FilterStart = null,
                    HandlerStart = instructions.GetLabel(handlerStart),
                    HandlerEnd = instructions.GetLabel(handlerEnd),
                    ExceptionType = exceptionType is not null ? importer.ImportType(exceptionType) : null
                };

                methodBody.ExceptionHandlers.Add(handler);
            }
        }

        public static object ResolveDynamicResolver(object dynamicMethodObj)
        {
            //Convert dynamicMethodObj to DynamicResolver
            if (dynamicMethodObj is Delegate del)
                dynamicMethodObj = del.Method;

            if (dynamicMethodObj is null)
                throw new ArgumentNullException(nameof(dynamicMethodObj));

            //We use GetType().FullName just to avoid the System.Reflection.Emit.LightWeight Dll
            if (dynamicMethodObj.GetType().FullName == "System.Reflection.Emit.DynamicMethod+RTDynamicMethod")
                dynamicMethodObj = FieldReader.ReadField<object>(dynamicMethodObj, "m_owner")!;

            if (dynamicMethodObj.GetType().FullName == "System.Reflection.Emit.DynamicMethod")
            {
                var resolver = FieldReader.ReadField<object>(dynamicMethodObj, "m_resolver");
                if (resolver != null)
                    dynamicMethodObj = resolver;
            }
            //Create Resolver if it does not exist.
            if (dynamicMethodObj.GetType().FullName == "System.Reflection.Emit.DynamicMethod")
            {
                var dynamicResolver = typeof(OpCode).Module.GetTypes()
                    .First(t => t.Name == "DynamicResolver");

                var ilGenerator = dynamicMethodObj.GetType().GetRuntimeMethods().First(q => q.Name == "GetILGenerator")
                    .Invoke(dynamicMethodObj, null);

                //Create instance of dynamicResolver
                dynamicMethodObj = Activator.CreateInstance(dynamicResolver, (BindingFlags)(-1), null, new[]
                {
                    ilGenerator
                }, null)!;
            }
            return dynamicMethodObj;
        }
    }
}
