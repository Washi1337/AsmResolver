using System;
using System.Collections.Generic;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Collections;
using AsmResolver.PE.DotNet.Cil;

namespace AsmResolver.DotNet.Cloning
{
    public partial class MetadataCloner
    {
        private void CloneMethodsInType(MetadataCloneContext context, TypeDefinition type)
        {
            CreateMethodStubsInType(context, type);
            DeepCopyMethodsInType(context, type);
        }

        private void CreateMethodStubsInType(MetadataCloneContext context, TypeDefinition type)
        {
            foreach (var method in type.Methods)
                CreateMethodStub(context, method);
        }

        private void CreateMethodStub(MetadataCloneContext context, MethodDefinition method)
        {
            var clonedMethod = new MethodDefinition(method.Name, method.Attributes,
                context.Importer.ImportMethodSignature(method.Signature));
            clonedMethod.ImplAttributes = method.ImplAttributes;

            clonedMethod.Parameters.PullUpdatesFromMethodSignature();

            CloneParameterDefinitionsInMethod(method, clonedMethod);

            var clonedType = (TypeDefinition) context.ClonedMembers[method.DeclaringType];
            clonedType.Methods.Add(clonedMethod);

            context.ClonedMembers[method] = clonedMethod;
        }

        private void CloneParameterDefinitionsInMethod(MethodDefinition method, MethodDefinition clonedMethod)
        {
            foreach (var parameterDef in method.ParameterDefinitions)
                clonedMethod.ParameterDefinitions.Add(CloneParameterDefinition(parameterDef));
        }

        private ParameterDefinition CloneParameterDefinition(ParameterDefinition parameterDef)
        {
            var clonedParameterDef = new ParameterDefinition(parameterDef.Sequence, parameterDef.Name, parameterDef.Attributes);
            return clonedParameterDef;
        }

        private void DeepCopyMethodsInType(MetadataCloneContext context, TypeDefinition type)
        {
            foreach (var method in type.Methods)
                DeepCopyMethod(context, method);
        }

        private void DeepCopyMethod(MetadataCloneContext context, MethodDefinition method)
        {
            var clonedMethod = (MethodDefinition) context.ClonedMembers[method];
            
            if (method.CilMethodBody != null)
                clonedMethod.CilMethodBody = CloneCilMethodBody(context, method);
        }

        private CilMethodBody CloneCilMethodBody(MetadataCloneContext context, MethodDefinition method)
        {
            var body = method.CilMethodBody;
            
            var clonedMethod = (MethodDefinition) context.ClonedMembers[method];
            var clonedBody = new CilMethodBody(clonedMethod);

            clonedBody.InitializeLocals = body.InitializeLocals;
            clonedBody.MaxStack = body.MaxStack;

            CloneLocalVariables(context, body, clonedBody);
            CloneCilInstructions(context, body, clonedBody);
            CloneExceptionHandlers(context, body, clonedBody);
            
            return clonedBody;
        }

        private void CloneLocalVariables(MetadataCloneContext context, CilMethodBody body, CilMethodBody clonedBody)
        {
            foreach (var variable in body.LocalVariables)
            {
                var clonedVariable = new CilLocalVariable(context.Importer.ImportTypeSignature(variable.VariableType));
                clonedBody.LocalVariables.Add(clonedVariable);
            }
        }

        private void CloneCilInstructions(MetadataCloneContext context, CilMethodBody body, CilMethodBody clonedBody)
        {
            var branches = new List<CilInstruction>();
            var switches = new List<CilInstruction>();

            foreach (var instruction in body.Instructions)
            {
                var clonedInstruction = CloneInstruction(context, clonedBody, instruction);
                if (clonedInstruction.IsBranch())
                    branches.Add(clonedInstruction);
                else if (clonedInstruction.OpCode.Code == CilCode.Switch)
                    switches.Add(clonedInstruction);
                clonedBody.Instructions.Add(clonedInstruction);
            }

            foreach (var branch in branches)
            {
                var label = (ICilLabel) branch.Operand;
                branch.Operand = new CilInstructionLabel(body.Instructions.GetByOffset(label.Offset));
            }

            foreach (var @switch in switches)
            {
                var labels = (IEnumerable<ICilLabel>) @switch.Operand;
                var clonedLabels = new List<ICilLabel>();
                foreach (var label in labels)
                    clonedLabels.Add(new CilInstructionLabel(body.Instructions.GetByOffset(label.Offset)));
            }
        }

        private CilInstruction CloneInstruction(MetadataCloneContext context, CilMethodBody clonedBody, CilInstruction instruction)
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
                    clonedInstruction.Operand = context.Importer.ImportField((IFieldDescriptor) instruction.Operand);
                    break;

                case CilOperandType.InlineMethod:
                    clonedInstruction.Operand = context.Importer.ImportMethod((IMethodDefOrRef) instruction.Operand);
                    break;

                case CilOperandType.InlineSig:
                    throw new NotImplementedException();

                case CilOperandType.InlineTok:
                    clonedInstruction.Operand = CloneInlineTokOperand(context, instruction);
                    break;

                case CilOperandType.InlineType:
                    clonedInstruction.Operand = context.Importer.ImportType((ITypeDefOrRef) instruction.Operand);
                    break;

                case CilOperandType.InlineVar:
                case CilOperandType.ShortInlineVar:
                    clonedInstruction.Operand = clonedBody.LocalVariables[((CilLocalVariable) instruction.Operand).Index];
                    break;

                case CilOperandType.InlineArgument:
                case CilOperandType.ShortInlineArgument:
                    clonedInstruction.Operand = clonedBody.Owner.Parameters[((Parameter) instruction.Operand).Index];
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return clonedInstruction;
        }

        private object CloneInlineTokOperand(MetadataCloneContext context, CilInstruction instruction)
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

        private static void CloneExceptionHandlers(MetadataCloneContext context, CilMethodBody body, CilMethodBody clonedBody)
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