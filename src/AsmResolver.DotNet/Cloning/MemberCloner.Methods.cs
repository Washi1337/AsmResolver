using System;
using System.Collections.Generic;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Collections;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;

namespace AsmResolver.DotNet.Cloning
{
    public partial class MemberCloner
    {
        private void CreateMethodStubs(MemberCloneContext context)
        {
            foreach (var method in _methodsToClone)
            {
                var stub = CreateMethodStub(context, method);

                // If method's declaring type is cloned as well, add the cloned method to the cloned type.
                if (method.DeclaringType is not null
                    && context.ClonedMembers.TryGetValue(method.DeclaringType, out var member)
                    && member is TypeDefinition declaringType)
                {
                    declaringType.Methods.Add(stub);
                }
            }
        }

        private static MethodDefinition CreateMethodStub(MemberCloneContext context, MethodDefinition method)
        {
            if (method.Name is null)
                throw new ArgumentException($"Method {method.SafeToString()} has no name.");
            if (method.Signature is null)
                throw new ArgumentException($"Method {method.SafeToString()} has no signature.");

            var clonedMethod = new MethodDefinition(method.Name, method.Attributes,
                context.Importer.ImportMethodSignature(method.Signature));
            clonedMethod.ImplAttributes = method.ImplAttributes;

            clonedMethod.Parameters.PullUpdatesFromMethodSignature();

            context.ClonedMembers[method] = clonedMethod;
            return clonedMethod;
        }

        private static ParameterDefinition CloneParameterDefinition(MemberCloneContext context, ParameterDefinition parameterDef)
        {
            var clonedParameterDef = new ParameterDefinition(parameterDef.Sequence, parameterDef.Name, parameterDef.Attributes);
            CloneCustomAttributes(context, parameterDef, clonedParameterDef);
            clonedParameterDef.Constant = CloneConstant(parameterDef.Constant);
            clonedParameterDef.MarshalDescriptor = CloneMarshalDescriptor(context, parameterDef.MarshalDescriptor);
            return clonedParameterDef;
        }

        private void DeepCopyMethods(MemberCloneContext context)
        {
            foreach (var method in _methodsToClone)
            {
                DeepCopyMethod(context, method);
                var clonedMember = (MethodDefinition)context.ClonedMembers[method];
                _clonerListener.OnClonedMember(method, clonedMember);
                if (_clonerListener is MemberClonerListener listener)
                    listener.OnClonedMethod(method, clonedMember);
            }
        }

        private void DeepCopyMethod(MemberCloneContext context, MethodDefinition method)
        {
            var clonedMethod = (MethodDefinition) context.ClonedMembers[method];

            CloneParameterDefinitionsInMethod(context, method, clonedMethod);

            if (method.CilMethodBody is not null)
                clonedMethod.CilMethodBody = CloneCilMethodBody(context, method);

            CloneCustomAttributes(context, method, clonedMethod);
            CloneGenericParameters(context, method, clonedMethod);
            CloneSecurityDeclarations(context, method, clonedMethod);

            clonedMethod.ImplementationMap = CloneImplementationMap(context, method.ImplementationMap);
        }

        private void CloneParameterDefinitionsInMethod(MemberCloneContext context, MethodDefinition method, MethodDefinition clonedMethod)
        {
            foreach (var parameterDef in method.ParameterDefinitions)
                clonedMethod.ParameterDefinitions.Add(CloneParameterDefinition(context, parameterDef));
        }

        private static CilMethodBody CloneCilMethodBody(MemberCloneContext context, MethodDefinition method)
        {
            var body = method.CilMethodBody!;

            var clonedMethod = (MethodDefinition) context.ClonedMembers[method];

            // Clone method body header.
            var clonedBody = new CilMethodBody(clonedMethod);
            clonedBody.InitializeLocals = body.InitializeLocals;
            clonedBody.MaxStack = body.MaxStack;

            // Clone contents.
            CloneLocalVariables(context, body, clonedBody);
            CloneCilInstructions(context, body, clonedBody);
            CloneExceptionHandlers(context, body, clonedBody);

            return clonedBody;
        }

        private static void CloneLocalVariables(MemberCloneContext context, CilMethodBody body, CilMethodBody clonedBody)
        {
            foreach (var variable in body.LocalVariables)
            {
                var clonedVariable = new CilLocalVariable(context.Importer.ImportTypeSignature(variable.VariableType));
                clonedBody.LocalVariables.Add(clonedVariable);
            }
        }

        private static void CloneCilInstructions(MemberCloneContext context, CilMethodBody body, CilMethodBody clonedBody)
        {
            var branches = new List<CilInstruction>();
            var switches = new List<CilInstruction>();

            // Clone all instructions.
            foreach (var instruction in body.Instructions)
            {
                var clonedInstruction = CloneInstruction(context, clonedBody, instruction);
                if (clonedInstruction.OpCode.Code == CilCode.Switch)
                    switches.Add(clonedInstruction);
                else if (clonedInstruction.IsBranch())
                    branches.Add(clonedInstruction);
                clonedBody.Instructions.Add(clonedInstruction);
            }

            // Fixup branches.
            foreach (var branch in branches)
            {
                var label = (ICilLabel) branch.Operand!;
                branch.Operand = clonedBody.Instructions.GetLabel(label.Offset);
            }

            // Fixup switches.
            foreach (var @switch in switches)
            {
                var labels = (IList<ICilLabel>) @switch.Operand!;
                var clonedLabels = new List<ICilLabel>(labels.Count);

                for (int i = 0; i < labels.Count; i++)
                    clonedLabels.Add(clonedBody.Instructions.GetLabel(labels[i].Offset));

                @switch.Operand = clonedLabels;
            }
        }

        private static CilInstruction CloneInstruction(MemberCloneContext context, CilMethodBody clonedBody, CilInstruction instruction)
        {
            var clonedInstruction = new CilInstruction(instruction.Offset, instruction.OpCode);
            switch (instruction.OpCode.OperandType)
            {
                case CilOperandType.InlineBrTarget:
                case CilOperandType.ShortInlineBrTarget:
                case CilOperandType.InlineSwitch:
                    // Fix up later when all instructions are added.
                    clonedInstruction.Operand = instruction.Operand;
                    break;

                case CilOperandType.InlineField when instruction.Operand is IFieldDescriptor field:
                    clonedInstruction.Operand = context.Importer.ImportField(field);
                    break;

                case CilOperandType.InlineMethod when instruction.Operand is IMethodDescriptor method:
                    clonedInstruction.Operand = context.Importer.ImportMethod(method);
                    break;

                case CilOperandType.InlineSig when instruction.Operand is StandAloneSignature standAlone:
                    instruction.Operand = new StandAloneSignature(standAlone.Signature switch
                    {
                        MethodSignature signature => context.Importer.ImportMethodSignature(signature),
                        GenericInstanceMethodSignature signature => context.Importer.ImportGenericInstanceMethodSignature(signature),
                        _ => throw new NotImplementedException()
                    });
                    break;

                case CilOperandType.InlineType when instruction.Operand is ITypeDefOrRef type:
                    clonedInstruction.Operand = context.Importer.ImportType(type);
                    break;

                case CilOperandType.InlineI:
                case CilOperandType.InlineI8:
                case CilOperandType.InlineNone:
                case CilOperandType.InlineR:
                case CilOperandType.InlineString:
                case CilOperandType.ShortInlineI:
                case CilOperandType.ShortInlineR:
                case CilOperandType.InlineField:
                case CilOperandType.InlineMethod:
                case CilOperandType.InlineType:
                    clonedInstruction.Operand = instruction.Operand;
                    break;

                case CilOperandType.InlineTok:
                    clonedInstruction.Operand = CloneInlineTokOperand(context, instruction);
                    break;

                case CilOperandType.InlineVar:
                case CilOperandType.ShortInlineVar:
                    clonedInstruction.Operand = instruction.Operand is CilLocalVariable local
                        ? clonedBody.LocalVariables[local.Index]
                        : instruction.Operand;
                    break;

                case CilOperandType.InlineArgument:
                case CilOperandType.ShortInlineArgument:
                    clonedInstruction.Operand = instruction.Operand is Parameter parameter
                        ? clonedBody.Owner.Parameters.GetBySignatureIndex(parameter.MethodSignatureIndex)
                        : instruction.Operand;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return clonedInstruction;
        }

        private static object CloneInlineTokOperand(MemberCloneContext context, CilInstruction instruction)
        {
            return instruction.Operand switch
            {
                ITypeDefOrRef type => context.Importer.ImportType(type),
                MemberReference {IsField: true} method => context.Importer.ImportField(method),
                MemberReference {IsMethod: true} field => context.Importer.ImportMethod(field),
                MethodDefinition method => context.Importer.ImportMethod(method),
                FieldDefinition field => context.Importer.ImportField(field),
                _ => throw new NotSupportedException()
            };
        }

        private static void CloneExceptionHandlers(MemberCloneContext context, CilMethodBody body, CilMethodBody clonedBody)
        {
            foreach (var handler in body.ExceptionHandlers)
            {
                clonedBody.ExceptionHandlers.Add(new CilExceptionHandler
                {
                    HandlerType = handler.HandlerType,
                    TryStart = ToClonedLabel(handler.TryStart),
                    TryEnd = ToClonedLabel(handler.TryEnd),
                    HandlerStart = ToClonedLabel(handler.HandlerStart),
                    HandlerEnd = ToClonedLabel(handler.HandlerEnd),
                    FilterStart = ToClonedLabel(handler.FilterStart),
                    ExceptionType = handler.ExceptionType is null
                        ? null
                        : context.Importer.ImportType(handler.ExceptionType)
                });
            }

            ICilLabel? ToClonedLabel(ICilLabel? label) => label is not null
                ? clonedBody.Instructions.GetLabel(label.Offset)
                : null;
        }
    }
}
