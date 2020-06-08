using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet
{
    internal static class DynamicMethodHelper
    {
        public static void ReadLocalVariables(this CilMethodBody methodBody, MethodDefinition method, byte[] localSig)
        {
            var locals = CallingConventionSignature.FromReader(method.Module, new ByteArrayReader(localSig)) as LocalVariablesSignature;
            for (var i = 0; i < locals?.VariableTypes.Count; i++)
                methodBody.LocalVariables.Add(new CilLocalVariable(locals.VariableTypes[i]));
        }
        public static void ReadReflectionExceptionHandlers(this CilMethodBody methodBody,
            IList<object> ehInfos, byte[] ehHeader, ReferenceImporter importer)
        {
            if (ehHeader != null && ehHeader.Length > 4)
                //Sample needed!
                throw new NotImplementedException("Exception Handlers From ehHeader Not Supported Yet.");
            if (ehInfos != null && ehInfos.Count > 0)
                for (var i = 0; i < ehInfos.Count; i++)
                {
                    //Get ExceptionHandlerInfo Field Values
                    var endFinally = FieldReader.ReadField<int>(ehInfos[i], "m_endFinally");
                    var endFinallyLabel = endFinally < 0
                        ? null
                        : methodBody.Instructions.GetByOffset(endFinally)?.CreateLabel() ??
                          new CilOffsetLabel(endFinally);
                    var endTry = FieldReader.ReadField<int>(ehInfos[i], "m_endAddr");
                    var endTryLabel = methodBody.Instructions.GetByOffset(endTry)?.CreateLabel() ??
                                      new CilOffsetLabel(endTry);
                    var handlerEnd = FieldReader.ReadField<int[]>(ehInfos[i], "m_catchEndAddr")[i];
                    var exceptionType = FieldReader.ReadField<Type[]>(ehInfos[i], "m_catchClass")[i];
                    var handlerStart = FieldReader.ReadField<int[]>(ehInfos[i], "m_catchAddr")[i];
                    var tryStart = FieldReader.ReadField<int>(ehInfos[i], "m_startAddr");
                    var handlerType = (CilExceptionHandlerType) FieldReader.ReadField<int[]>(ehInfos[i], "m_type")[i];

                    //Create the handler
                    var handler = new CilExceptionHandler
                    {
                        HandlerType = handlerType,
                        TryStart = methodBody.Instructions.GetByOffset(tryStart)?.CreateLabel() ??
                                   new CilOffsetLabel(tryStart),
                        TryEnd = handlerType == CilExceptionHandlerType.Finally ? endFinallyLabel : endTryLabel,
                        FilterStart = null,
                        HandlerStart = methodBody.Instructions.GetByOffset(handlerStart)?.CreateLabel() ??
                                       new CilOffsetLabel(handlerStart),
                        HandlerEnd = methodBody.Instructions.GetByOffset(handlerEnd)?.CreateLabel() ??
                                     new CilOffsetLabel(handlerEnd),
                        ExceptionType = importer.ImportType(exceptionType)
                    };

                    methodBody.ExceptionHandlers.Add(handler);
                }
        }
        public static object ResolveOperandReflection(this CilMethodBody methodBody, CilInstruction instruction,
            ICilOperandResolver resolver, List<object> Tokens, ReferenceImporter Importer)
        {
            switch (instruction.OpCode.OperandType)
            {
                case CilOperandType.InlineBrTarget:
                case CilOperandType.ShortInlineBrTarget:
                    return new CilInstructionLabel(
                        methodBody.Instructions.GetByOffset(((ICilLabel) instruction.Operand).Offset));
                case CilOperandType.InlineField:
                case CilOperandType.InlineMethod:
                case CilOperandType.InlineSig:
                case CilOperandType.InlineTok:
                case CilOperandType.InlineType:
                    return ReadToken(((MetadataToken) instruction.Operand).ToUInt32(), Tokens, Importer);
                case CilOperandType.InlineString:
                    return ReadToken(((MetadataToken) instruction.Operand).ToUInt32(), Tokens, Importer);
                case CilOperandType.InlineSwitch:
                    var result = new List<ICilLabel>();
                    var labels = (IEnumerable<ICilLabel>) instruction.Operand;
                    foreach (var label in labels)
                    {
                        var target = methodBody.Instructions.GetByOffset(label.Offset);
                        result.Add(target == null ? label : new CilInstructionLabel(target));
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

        private static object ReadToken(MetadataToken token, IList<object> tokens, ReferenceImporter importer)
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
                        return importer.ImportField(FieldInfo.GetFieldFromHandle(
                            FieldReader.ReadField<RuntimeFieldHandle>(field, "m_field"),
                            FieldReader.ReadField<RuntimeTypeHandle>(field, "m_context")));
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
                        var method = MethodBase.GetMethodFromHandle(
                            FieldReader.ReadField<RuntimeMethodHandle>(obj, "m_method"), context);
                        return importer.ImportMethod(method);
                    }

                    if (obj.GetType().FullName == "System.Reflection.Emit.VarArgMethod")
                        return importer.ImportMethod(FieldReader.ReadField<MethodInfo>(obj, "m_method"));
                    break;
                case TableIndex.StandAloneSig:
                    return CallingConventionSignature.FromReader(importer.TargetModule,
                        new ByteArrayReader(tokens[(int) token.Rid] as byte[]));
                case (TableIndex)112:
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
            if (dynamicMethodObj.GetType().FullName == "System.Reflection.Emit.RTDynamicMethod")
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