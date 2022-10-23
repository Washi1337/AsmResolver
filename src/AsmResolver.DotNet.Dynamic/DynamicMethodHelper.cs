using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Serialized;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.IO;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Dynamic
{
    internal static class DynamicMethodHelper
    {
        private static readonly MethodInfo? GetTypeFromHandleUnsafeMethod;

        static DynamicMethodHelper()
        {
            // We need to use reflection for this to stay compatible with .netstandard 2.0.
            GetTypeFromHandleUnsafeMethod = typeof(Type)
                .GetMethod("GetTypeFromHandleUnsafe",
                    (BindingFlags) (-1),
                    null,
                    new[] {typeof(IntPtr)},
                    null);
        }

        [MemberNotNullWhen(true, nameof(GetTypeFromHandleUnsafeMethod))]
        public static bool IsSupported => GetTypeFromHandleUnsafeMethod is not null;

        public static void ReadLocalVariables(CilMethodBody methodBody, MethodDefinition method, byte[] localSig)
        {
            if (method.Module is not SerializedModuleDefinition module)
                throw new ArgumentException("Method body should reference a serialized module.");

            var reader = new BinaryStreamReader(localSig);
            if (ReadLocalVariableSignature(new BlobReadContext(module.ReaderContext), ref reader)
                is not { } localsSignature)
            {
                throw new ArgumentException("Invalid local variables signature.");
            }

            for (int i = 0; i < localsSignature.VariableTypes.Count; i++)
                methodBody.LocalVariables.Add(new CilLocalVariable(localsSignature.VariableTypes[i]));
        }

        private static TypeSignature ReadTypeSignature(in BlobReadContext context, ref BinaryStreamReader reader)
        {
            return (ElementType) reader.PeekByte() == ElementType.Internal
                ? ReadInternalTypeSignature(context, ref reader)
                : TypeSignature.FromReader(in context, ref reader);
        }

        private static TypeSignature ReadInternalTypeSignature(in BlobReadContext context, ref BinaryStreamReader reader)
        {
            if (!IsSupported)
                throw new PlatformNotSupportedException("The current platform does not support the translation of raw type handles to System.Type instances.");

            // Consume INTERNAL element type.
            reader.ReadByte();

            // Read address.
            var address = IntPtr.Size switch
            {
                4 => new IntPtr(reader.ReadInt32()),
                _ => new IntPtr(reader.ReadInt64())
            };

            // Let the runtime translate the address to a type and import it.
            var clrType = (Type?) GetTypeFromHandleUnsafeMethod.Invoke(null, new object[] { address });

            var type = clrType is not null
                ? new ReferenceImporter(context.ReaderContext.ParentModule).ImportType(clrType)
                : InvalidTypeDefOrRef.Get(InvalidTypeSignatureError.IllegalTypeSpec);

            return new TypeDefOrRefSignature(type);
        }

        private static LocalVariablesSignature ReadLocalVariableSignature(
            in BlobReadContext context,
            ref BinaryStreamReader reader)
        {
            var result = new LocalVariablesSignature();
            result.Attributes = (CallingConventionAttributes) reader.ReadByte();

            if (!reader.TryReadCompressedUInt32(out uint count))
            {
                context.ReaderContext.BadImage("Invalid number of local variables in local variable signature.");
                return result;
            }

            for (int i = 0; i < count; i++)
                result.VariableTypes.Add(ReadTypeSignature(context, ref reader));

            return result;
        }

        public static void ReadReflectionExceptionHandlers(CilMethodBody methodBody,
            byte[]? ehHeader, IList<object>? ehInfos, ReferenceImporter importer)
        {
            if (ehHeader is {Length: > 4})
            {
                InterpretEHSection(methodBody, importer, ehHeader);
            }
            else if (ehInfos is { Count: > 0 })
            {
                foreach (var ehInfo in ehInfos)
                    InterpretEHInfo(methodBody, importer, ehInfo);
            }
        }

        private static void InterpretEHSection(CilMethodBody methodBody, ReferenceImporter importer, byte[] ehHeader)
        {
            throw new NotImplementedException("Raw exception data is not supported yet.");
        }

        private static void InterpretEHInfo(CilMethodBody methodBody, ReferenceImporter importer, object ehInfo)
        {
            for (int i = 0; i < FieldReader.ReadField<int>(ehInfo, "m_currentCatch"); i++)
            {
                // Get ExceptionHandlerInfo Field Values
                int endFinally = FieldReader.ReadField<int>(ehInfo, "m_endFinally");
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
                object? resolver = FieldReader.ReadField<object>(dynamicMethodObj, "m_resolver");
                if (resolver != null)
                    dynamicMethodObj = resolver;
            }
            //Create Resolver if it does not exist.
            if (dynamicMethodObj.GetType().FullName == "System.Reflection.Emit.DynamicMethod")
            {
                var dynamicResolver = typeof(OpCode).Module
                    .GetTypes()
                    .First(t => t.Name == "DynamicResolver");

                object? ilGenerator = dynamicMethodObj
                    .GetType()
                    .GetRuntimeMethods()
                    .First(q => q.Name == "GetILGenerator")
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
