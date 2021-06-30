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
                if (context.ClonedMembers.TryGetValue(method.DeclaringType, out var member)
                    && member is TypeDefinition declaringType)
                {
                    declaringType.Methods.Add(stub);
                }
            }
        }

        private static MethodDefinition CreateMethodStub(MemberCloneContext context, MethodDefinition method)
        {
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
            clonedParameterDef.Constant = CloneConstant(context, parameterDef.Constant);
            clonedParameterDef.MarshalDescriptor = CloneMarshalDescriptor(context, parameterDef.MarshalDescriptor);
            return clonedParameterDef;
        }

        private void DeepCopyMethods(MemberCloneContext context)
        {
            foreach (var method in _methodsToClone)
                DeepCopyMethod(context, method);
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
                var label = (ICilLabel) branch.Operand;
                branch.Operand = new CilInstructionLabel(clonedBody.Instructions.GetByOffset(label.Offset));
            }

            // Fixup switches.
            foreach (var @switch in switches)
            {
                var labels = (IList<ICilLabel>) @switch.Operand;
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

                case CilOperandType.InlineI:
                case CilOperandType.InlineI8:
                case CilOperandType.InlineNone:
                case CilOperandType.InlineR:
                case CilOperandType.InlineString:
                case CilOperandType.ShortInlineI:
                case CilOperandType.ShortInlineR:
                    clonedInstruction.Operand = instruction.Operand;
                    break;

                case CilOperandType.InlineField:
                    clonedInstruction.Operand = context.Importer.ImportField((IFieldDescriptor)instruction.Operand);
                    break;

                case CilOperandType.InlineMethod:
                    clonedInstruction.Operand = context.Importer.ImportMethod((IMethodDescriptor)instruction.Operand);
                    break;

                case CilOperandType.InlineSig:
                    if (instruction.Operand is StandAloneSignature standalone)
                    {
                        instruction.Operand = new StandAloneSignature(standalone.Signature switch
                        {
                            MethodSignature signature => context.Importer.ImportMethodSignature(signature),
                            GenericInstanceMethodSignature signature => context.Importer.ImportGenericInstanceMethodSignature(signature),
                            _ => throw new NotImplementedException()
                        });
                    }
                    else
                        throw new NotImplementedException();
                    break;
                case CilOperandType.InlineTok:
                    clonedInstruction.Operand = CloneInlineTokOperand(context, instruction);
                    break;

                case CilOperandType.InlineType:
                    clonedInstruction.Operand = context.Importer.ImportType((ITypeDefOrRef)instruction.Operand);
                    break;

                case CilOperandType.InlineVar:
                case CilOperandType.ShortInlineVar:
                    clonedInstruction.Operand = clonedBody.LocalVariables[((CilLocalVariable)instruction.Operand).Index];
                    break;

                case CilOperandType.InlineArgument:
                case CilOperandType.ShortInlineArgument:
                    clonedInstruction.Operand = clonedBody.Owner.Parameters.GetBySignatureIndex(((Parameter)instruction.Operand).MethodSignatureIndex);
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
                    TryStart = clonedBody.Instructions.GetLabel(handler.TryStart.Offset),
                    TryEnd = clonedBody.Instructions.GetLabel(handler.TryEnd.Offset),
                    HandlerStart = clonedBody.Instructions.GetLabel(handler.HandlerStart.Offset),
                    HandlerEnd = clonedBody.Instructions.GetLabel(handler.HandlerEnd.Offset),
                    FilterStart = handler.FilterStart is { } filterStart
                        ? clonedBody.Instructions.GetLabel(filterStart.Offset)
                        : null,
                    ExceptionType = handler.ExceptionType is null
                        ? null
                        : context.Importer.ImportType(handler.ExceptionType)
                });
            }
        }
    }
}
