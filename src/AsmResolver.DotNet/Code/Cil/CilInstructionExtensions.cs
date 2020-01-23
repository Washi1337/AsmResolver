using System;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;

namespace AsmResolver.DotNet.Code.Cil
{
    /// <summary>
    /// Provides extensions to the <see cref="CilInstruction"/> class.
    /// </summary>
    public static class CilInstructionExtensions
    {
        /// <summary>
        /// Determines the number of values that are popped from the stack by this instruction.
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <param name="parent">The method body that this instruction resides in. When passed on <c>null</c>,
        /// a method body of a System.Void method is assumed.</param>
        /// <returns>The number of values popped from the stack.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Occurs when the instruction's operation code provides an
        /// invalid stack behaviour.</exception>
        public static int GetStackPopCount(this CilInstruction instruction, CilMethodBody parent)
        {
            switch (instruction.OpCode.StackBehaviourPop)
            {
                case CilStackBehaviour.Pop0:
                    return 0;
                
                case CilStackBehaviour.Pop1:
                case CilStackBehaviour.PopI:
                case CilStackBehaviour.PopRef:
                    return 1;
                
                case CilStackBehaviour.Pop1_Pop1:
                case CilStackBehaviour.PopI_Pop1:
                case CilStackBehaviour.PopI_PopI:
                case CilStackBehaviour.PopI_PopI8:
                case CilStackBehaviour.PopI_PopR4:
                case CilStackBehaviour.PopI_PopR8:
                case CilStackBehaviour.PopRef_Pop1:
                case CilStackBehaviour.PopRef_PopI:
                    return 2;
                
                case CilStackBehaviour.PopI_PopI_PopI:
                case CilStackBehaviour.PopRef_PopI_PopI:
                case CilStackBehaviour.PopRef_PopI_PopI8:
                case CilStackBehaviour.PopRef_PopI_PopR4:
                case CilStackBehaviour.PopRef_PopI_PopR8:
                case CilStackBehaviour.PopRef_PopI_PopRef:
                case CilStackBehaviour.PopRef_PopI_Pop1:
                    return 3;
                
                case CilStackBehaviour.VarPop:
                    return DetermineVarPopCount(instruction, parent);
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static int DetermineVarPopCount(CilInstruction instruction, CilMethodBody parent)
        {
            var opCode = instruction.OpCode;
            var signature = instruction.Operand switch
            {
                IMethodDescriptor method => method.Signature,
                StandAloneSignature standAloneSig => standAloneSig.Signature as MethodSignature,
                _ => null
            };

            if (signature == null)
            {
                if (opCode.Code == CilCode.Ret)
                    return parent == null || parent.Owner.Signature.ReturnType.IsTypeOf("System", "Void") ? 0 : 1;
            }
            else
            {
                int count = signature.ParameterTypes.Count;
                if (signature.HasThis && opCode.Code != CilCode.Newobj)
                    count++;
                return count;
            }

            return 0;
        }

        /// <summary>
        /// Determines the number of values that are pushed onto the stack by this instruction.
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <returns>The number of values pushed onto the stack.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Occurs when the instruction's operation code provides an
        /// invalid stack behaviour.</exception>
        public static int GetStackPushCount(this CilInstruction instruction)
        {
            switch (instruction.OpCode.StackBehaviourPush)
            {
                case CilStackBehaviour.Push0:
                    return 0;
                
                case CilStackBehaviour.Push1:
                case CilStackBehaviour.PushI:
                case CilStackBehaviour.PushI8:
                case CilStackBehaviour.PushR4:
                case CilStackBehaviour.PushR8:
                case CilStackBehaviour.PushRef:
                    return 1;
                
                case CilStackBehaviour.Push1_Push1:
                    return 2;
                
                case CilStackBehaviour.VarPush:
                    return DetermineVarPushCount(instruction);
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static int DetermineVarPushCount(CilInstruction instruction)
        {
            var signature = instruction.Operand switch
            {
                IMethodDescriptor method => method.Signature,
                StandAloneSignature standAloneSig => standAloneSig.Signature as MethodSignature,
                _ => null
            };

            if (signature == null)
                return 0;
            
            if (!signature.ReturnType.IsTypeOf("System", "Void") || instruction.OpCode.Code == CilCode.Newobj)
                return 1;

            return 0;
        }
        
    }
}