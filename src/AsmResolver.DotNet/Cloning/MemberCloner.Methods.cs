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
        
        private MethodDefinition CreateMethodStub(MemberCloneContext context, MethodDefinition method)
        {
            var clonedMethod = new MethodDefinition(method.Name, method.Attributes,
                context.Importer.ImportMethodSignature(method.Signature));
            clonedMethod.ImplAttributes = method.ImplAttributes;

            clonedMethod.Parameters.PullUpdatesFromMethodSignature();

            context.ClonedMembers[method] = clonedMethod;
            return clonedMethod;
        }

        private ParameterDefinition CloneParameterDefinition(MemberCloneContext context, ParameterDefinition parameterDef)
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
            
            if (method.CilMethodBody != null)
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

        private CilMethodBody CloneCilMethodBody(MemberCloneContext context, MethodDefinition method)
        {
            var body = method.CilMethodBody;
            
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

        private void CloneLocalVariables(MemberCloneContext context, CilMethodBody body, CilMethodBody clonedBody)
        {
            foreach (var variable in body.LocalVariables)
            {
                var clonedVariable = new CilLocalVariable(context.Importer.ImportTypeSignature(variable.VariableType));
                clonedBody.LocalVariables.Add(clonedVariable);
            }
        }

        private void CloneCilInstructions(MemberCloneContext context, CilMethodBody body, CilMethodBody clonedBody)
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
                var labels = (ICollection<ICilLabel>) @switch.Operand;
                var clonedLabels = new List<ICilLabel>(labels.Count);
                foreach (var label in labels)
                    clonedLabels.Add(new CilInstructionLabel(clonedBody.Instructions.GetByOffset(label.Offset)));
                @switch.Operand = clonedLabels;

            }
        }

        private CilInstruction CloneInstruction(MemberCloneContext context, CilMethodBody clonedBody, CilInstruction instruction)
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
                    clonedInstruction.Operand = clonedBody.Owner.Parameters[((Parameter)instruction.Operand).Index];
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return clonedInstruction;
        }

        private object CloneInlineTokOperand(MemberCloneContext context, CilInstruction instruction)
        {
            return instruction.Operand switch
            {
                ITypeDefOrRef type => (object) context.Importer.ImportType(type),
                MemberReference member when member.IsField => context.Importer.ImportField(member),
                MemberReference member when member.IsMethod => context.Importer.ImportMethod(member),
                MethodDefinition m => context.Importer.ImportMethod(m),
                FieldDefinition field => context.Importer.ImportField(field),
                _ => throw new NotSupportedException()
            };
        }

        private static void CloneExceptionHandlers(MemberCloneContext context, CilMethodBody body, CilMethodBody clonedBody)
        {
            foreach (var handler in body.ExceptionHandlers)
            {
                var clonedHandler = new CilExceptionHandler
                {
                    HandlerType = handler.HandlerType,
                    ExceptionType = handler.ExceptionType is null ? null : context.Importer.ImportType(handler.ExceptionType),
                    TryStart = clonedBody.Instructions.GetByOffset(handler.TryStart.Offset).CreateLabel(),
                    TryEnd = clonedBody.Instructions.GetByOffset(handler.TryEnd.Offset).CreateLabel(),
                    HandlerStart = clonedBody.Instructions.GetByOffset(handler.HandlerStart.Offset).CreateLabel(),
                    HandlerEnd = clonedBody.Instructions.GetByOffset(handler.HandlerEnd.Offset).CreateLabel(),
                    FilterStart = handler.FilterStart is null
                        ? null
                        : clonedBody.Instructions.GetByOffset(handler.FilterStart.Offset).CreateLabel()
                };
                clonedBody.ExceptionHandlers.Add(clonedHandler);
            }
        }
    }
}