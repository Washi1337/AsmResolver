using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Serialized;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet
{
    internal static class DynamicMethodHelper
    {
        public static void ReadLocalVariables(CilMethodBody methodBody, MethodDefinition method, byte[] localSig)
        {
            if (!(method.Module is SerializedModuleDefinition module))
                throw new ArgumentException("Method body should reference a serialized module.");

            var localsSignature = (LocalVariablesSignature) CallingConventionSignature.FromReader(
                new BlobReadContext(module.ReaderContext),
                new ByteArrayReader(localSig));

            for (int i = 0; i < localsSignature?.VariableTypes.Count; i++)
                methodBody.LocalVariables.Add(new CilLocalVariable(localsSignature.VariableTypes[i]));
        }

        public static void ReadReflectionExceptionHandlers(CilMethodBody methodBody,
            IList<object> ehInfos, byte[] ehHeader, ReferenceImporter importer)
        {
            //Sample needed!
            if (ehHeader != null && ehHeader.Length > 4)
                throw new NotImplementedException("Exception handlers from ehHeader not supported yet.");

            if (ehInfos != null && ehInfos.Count > 0)
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
                int handlerStart = FieldReader.ReadField<int[]>(ehInfo, "m_catchAddr")[i];
                int handlerEnd = FieldReader.ReadField<int[]>(ehInfo, "m_catchEndAddr")[i];
                var exceptionType = FieldReader.ReadField<Type[]>(ehInfo, "m_catchClass")[i];
                var handlerType = (CilExceptionHandlerType) FieldReader.ReadField<int[]>(ehInfo, "m_type")[i];

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
                    ExceptionType = exceptionType != null ? importer.ImportType(exceptionType) : null
                };

                methodBody.ExceptionHandlers.Add(handler);
            }
        }

        public static object ResolveOperandReflection(ModuleReaderContext context, CilMethodBody methodBody, CilInstruction instruction,
            ICilOperandResolver resolver, List<object> tokens, ReferenceImporter importer)
        {
            switch (instruction.OpCode.OperandType)
            {
                case CilOperandType.InlineBrTarget:
                case CilOperandType.ShortInlineBrTarget:
                    return methodBody.Instructions
                        .GetByOffset(((ICilLabel) instruction.Operand).Offset)
                        ?.CreateLabel();

                case CilOperandType.InlineField:
                case CilOperandType.InlineMethod:
                case CilOperandType.InlineSig:
                case CilOperandType.InlineTok:
                case CilOperandType.InlineType:
                    return ReadToken(context, ((MetadataToken) instruction.Operand).ToUInt32(), tokens, importer);

                case CilOperandType.InlineString:
                    return ReadToken(context, ((MetadataToken) instruction.Operand).ToUInt32(), tokens, importer);

                case CilOperandType.InlineSwitch:
                    var result = new List<ICilLabel>();
                    var labels = (IList<ICilLabel>) instruction.Operand;
                    for (int i = 0; i < labels.Count; i++)
                    {
                        var label = labels[i];
                        var target = methodBody.Instructions.GetByOffset(label.Offset);
                        result.Add(target != null
                            ? new CilInstructionLabel(target)
                            : label);
                    }

                    return result;

                case CilOperandType.InlineVar:
                case CilOperandType.ShortInlineVar:
                    return resolver.ResolveLocalVariable(Convert.ToInt32(instruction.Operand));

                case CilOperandType.InlineArgument:
                case CilOperandType.ShortInlineArgument:
                    return resolver.ResolveParameter(Convert.ToInt32(instruction.Operand));

                case CilOperandType.InlineI:
                case CilOperandType.InlineI8:
                case CilOperandType.InlineNone:
                case CilOperandType.InlineR:
                case CilOperandType.ShortInlineI:
                case CilOperandType.ShortInlineR:
                    return instruction.Operand;

                case CilOperandType.InlinePhi:
                    throw new NotSupportedException();

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static object ReadToken(ModuleReaderContext readerContext, MetadataToken token, IList<object> tokens, ReferenceImporter importer)
        {
            switch (token.Table)
            {
                case TableIndex.TypeDef:
                    var type = tokens[(int) token.Rid];
                    if (type is RuntimeTypeHandle runtimeTypeHandle)
                        return importer.ImportType(Type.GetTypeFromHandle(runtimeTypeHandle));
                    break;

                case TableIndex.Field:
                    var field = tokens[(int) token.Rid];
                    if (field is null)
                        return null;
                    if (field is RuntimeFieldHandle runtimeFieldHandle)
                        return importer.ImportField(FieldInfo.GetFieldFromHandle(runtimeFieldHandle));

                    if (field.GetType().FullName == "System.Reflection.Emit.GenericFieldInfo")
                    {
                        var result = FieldReader.TryReadField<RuntimeFieldHandle>(field, "m_field", out var mField);
                        var ctx = FieldReader.ReadField<RuntimeTypeHandle>(field, "m_context");
                        return importer.ImportField(FieldInfo.GetFieldFromHandle(result
                            ? mField
                            : FieldReader.ReadField<RuntimeFieldHandle>(field, "m_fieldHandle"), ctx));
                    }

                    break;

                case TableIndex.Method:
                case TableIndex.MemberRef:
                    var obj = tokens[(int) token.Rid];
                    if (obj is RuntimeMethodHandle methodHandle)
                        return importer.ImportMethod(MethodBase.GetMethodFromHandle(methodHandle));
                    if (obj.GetType().FullName == "System.Reflection.Emit.GenericMethodInfo")
                    {
                        var context =
                            FieldReader.ReadField<RuntimeTypeHandle>(obj, "m_context");
                        var res = FieldReader.TryReadField<RuntimeMethodHandle>(obj, "m_method", out var m_method);
                        var m_handle = FieldReader.ReadField<RuntimeMethodHandle>(obj, "m_methodHandle");
                        var method = MethodBase.GetMethodFromHandle(
                            res
                                ? m_method
                                : m_handle, context);
                        return importer.ImportMethod(method);
                    }

                    if (obj.GetType().FullName == "System.Reflection.Emit.VarArgMethod")
                        return importer.ImportMethod(FieldReader.ReadField<MethodInfo>(obj, "m_method"));
                    break;

                case TableIndex.StandAloneSig:
                    return CallingConventionSignature.FromReader(
                        new BlobReadContext(readerContext),
                        new ByteArrayReader((byte[]) tokens[(int) token.Rid]));

                case (TableIndex) 112:
                    return tokens[(int) token.Rid] as string;
            }

            return null;
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
                dynamicMethodObj = FieldReader.ReadField<object>(dynamicMethodObj, "m_owner");

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
                dynamicMethodObj = Activator.CreateInstance(dynamicResolver, (BindingFlags) (-1), null, new[]
                {
                    ilGenerator
                }, null);
            }
            return dynamicMethodObj;
        }
    }
}
