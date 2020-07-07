using System;
using System.Collections.Generic;
using AsmResolver.DotNet.Collections;
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
        public static int GetStackPopCount(this CilInstruction instruction, CilMethodBody parent) =>
            GetStackPopCount(instruction,
                parent == null || parent.Owner.Signature.ReturnType.IsTypeOf("System", "Void"));
        
        /// <summary>
        /// Determines the number of values that are popped from the stack by this instruction.
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <param name="isVoid">A value indicating whether the enclosing method is returning System.Void or not.</param>
        /// <returns>The number of values popped from the stack.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Occurs when the instruction's operation code provides an
        /// invalid stack behaviour.</exception>
        public static int GetStackPopCount(this CilInstruction instruction, bool isVoid)
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
                    return DetermineVarPopCount(instruction, isVoid);
                
                case CilStackBehaviour.PopAll:
                    return -1;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static int DetermineVarPopCount(CilInstruction instruction, bool isVoid)
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
                    return isVoid ? 0 : 1;
            }
            else
            {
                int count = signature.GetTotalParameterCount();
                switch (opCode.Code)
                {
                    case CilCode.Newobj:
                        // NewObj produces instead of consumes the this parameter.
                        count--;
                        break;
                    
                    case CilCode.Calli:
                        // Calli consumes an extra parameter (the address to call).
                        count++;
                        break;
                }
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

        /// <summary>
        /// When this instruction is using a variant of the ldloc or stloc opcodes, gets the local variable that is
        /// referenced by the instruction.
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <param name="variables">The local variables defined in the enclosing method body.</param>
        /// <returns>The variable.</returns>
        /// <exception cref="ArgumentException">Occurs when the instruction is not using a variant of the ldloc or stloc opcodes.</exception>
        public static CilLocalVariable GetLocalVariable(this CilInstruction instruction, IReadOnlyList<CilLocalVariable> variables)
        {
            switch (instruction.OpCode.Code)
            {
                case CilCode.Ldloc:
                case CilCode.Ldloc_S:
                case CilCode.Stloc:
                case CilCode.Stloc_S:
                case CilCode.Ldloca:
                case CilCode.Ldloca_S:
                    return (CilLocalVariable) instruction.Operand;
                
                case CilCode.Ldloc_0:
                case CilCode.Stloc_0:
                    return variables[0];
                    
                case CilCode.Ldloc_1:
                case CilCode.Stloc_1:
                    return variables[1];
                    
                case CilCode.Ldloc_2:
                case CilCode.Stloc_2:
                    return variables[2];
                    
                case CilCode.Ldloc_3:
                case CilCode.Stloc_3:
                    return variables[3];
                
                default:
                    throw new ArgumentException("Instruction is not a ldloc or stloc instruction.");
            }
        }

        /// <summary>
        /// When this instruction is using a variant of the ldarg or starg opcodes, gets the parameter that is
        /// referenced by the instruction.
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <param name="parameters">The parameters defined in the enclosing method body.</param>
        /// <returns>The parameter.</returns>
        /// <exception cref="ArgumentException">Occurs when the instruction is not using a variant of the ldarg or starg opcodes.</exception>
        public static Parameter GetParameter(this CilInstruction instruction, ParameterCollection parameters)
        {
            switch (instruction.OpCode.Code)
            {
                case CilCode.Ldarg:
                case CilCode.Ldarg_S:
                case CilCode.Ldarga:
                case CilCode.Ldarga_S:
                case CilCode.Starg:
                case CilCode.Starg_S:
                    return (Parameter) instruction.Operand;
                
                case CilCode.Ldarg_0:
                    return parameters.GetBySignatureIndex(0);
                    
                case CilCode.Ldarg_1:
                    return parameters.GetBySignatureIndex(1);
                    
                case CilCode.Ldarg_2:
                    return parameters.GetBySignatureIndex(2);
                    
                case CilCode.Ldarg_3:
                    return parameters.GetBySignatureIndex(3);
                
                default:
                    throw new ArgumentException("Instruction is not a ldarg or starg instruction.");
            }
        }
        
    }
}