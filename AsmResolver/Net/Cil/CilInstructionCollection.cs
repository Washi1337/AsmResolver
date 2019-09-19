using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Cil
{
    /// <summary>
    /// Represents a collection of CIL instructions found in a method body.
    /// </summary>
    public class CilInstructionCollection : IList<CilInstruction>
    {
        private readonly List<CilInstruction> _items = new List<CilInstruction>();

        public CilInstructionCollection(CilMethodBody body)
        {
            Owner = body ?? throw new ArgumentNullException(nameof(body));
        }

        public CilMethodBody Owner
        {
            get;
        }

        /// <inheritdoc />
        public int Count => _items.Count;

        /// <inheritdoc />
        public bool IsReadOnly => false;

        /// <summary>
        /// Gets the size in bytes of the collection.
        /// </summary>
        public int Size => _items.Sum(x => x.Size);
        
        /// <inheritdoc />
        public CilInstruction this[int index]
        {
            get => _items[index];
            set => _items[index] = value;
        }
        
        /// <inheritdoc />
        public void Add(CilInstruction item)
        {
            _items.Add(item);
        }

        /// <summary>
        /// Adds a collection of CIL instructions to the end of the list.
        /// </summary>
        /// <param name="items">The instructions to add.</param>
        public void AddRange(IEnumerable<CilInstruction> items)
        {
            _items.AddRange(items);
        }

        /// <summary>
        /// Inserts a collection of CIL instructions at the provided index.
        /// </summary>
        /// <param name="index">The index to insert the instructions into.</param>
        /// <param name="items">The instructions to insert.</param>
        public void InsertRange(int index, IEnumerable<CilInstruction> items)
        {
            _items.InsertRange(index, items);
        }

        /// <inheritdoc />
        public void Clear()
        {
            _items.Clear();
        }

        /// <inheritdoc />
        public bool Contains(CilInstruction item)
        {
            return _items.Contains(item);
        }

        /// <inheritdoc />
        public void CopyTo(CilInstruction[] array, int arrayIndex)
        {
            _items.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc />
        public bool Remove(CilInstruction item)
        {
            return _items.Remove(item);
        }

        /// <inheritdoc />
        public int IndexOf(CilInstruction item)
        {
            return _items.IndexOf(item);
        }

        /// <inheritdoc />
        public void Insert(int index, CilInstruction item)
        {
            _items.Insert(index, item);
        }

        /// <inheritdoc />
        public void RemoveAt(int index)
        {
            _items.RemoveAt(index);
        }

        /// <inheritdoc />
        public IEnumerator<CilInstruction> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _items).GetEnumerator();
        }

        /// <summary>
        /// Searches for an instruction with the given offset.
        /// </summary>
        /// <param name="offset">The offset of the instruction to find.</param>
        /// <returns>The index the instruction is located at, or -1 if an instruction with the provided offset could not
        /// be found.</returns>
        /// <remarks>Requires the offsets of the instructions pre-calculated. This can be done by calling
        /// <see cref="CalculateOffsets"/> prior to calling this method.</remarks>
        public int GetIndexByOffset(int offset)
        {
            int left = 0;
            int right = Count - 1;

            while (left <= right)
            {
                int m = (left + right) / 2;
                int currentOffset = _items[m].Offset;

                if (currentOffset > offset)
                    right = m - 1;
                else if (currentOffset < offset)
                    left = m + 1;
                else
                    return m;
            }

            return -1;
        }
        
        /// <summary>
        /// Searches for an instruction with the given offset.
        /// </summary>
        /// <param name="offset">The offset of the instruction to find.</param>
        /// <returns>The instruction with the provided offset, or null if none could be found.</returns>
        /// <remarks>Requires the offsets of the instructions pre-calculated. This can be done by calling
        /// <see cref="CalculateOffsets"/> prior to calling this method.</remarks>
        public CilInstruction GetByOffset(int offset)
        {
            int index = GetIndexByOffset(offset);
            return index == -1 ? null : _items[index];
        }

        /// <summary>
        /// Calculates the offsets of each instruction in the list. 
        /// </summary>
        public void CalculateOffsets()
        {
            var currentOffset = 0;
            foreach (var instruction in _items)
            {
                instruction.Offset = currentOffset;
                currentOffset += instruction.Size;
            }
        }

        /// <summary>
        /// Simplifies the CIL instructions by transforming macro instructions to their expanded form.
        /// This method does the opposite of <see cref="OptimizeMacros"/>.
        /// </summary>
        /// <example>
        /// Every instruction with the OpCode ldc.i4.0 will be converted to an ldc.i4 instruction with operand 0. 
        /// </example>
        public void ExpandMacros()
        {
            foreach (var instruction in _items)
                ExpandMacro(instruction);
            CalculateOffsets();
        }

        /// <summary>
        /// Simplifies the CIL instruction when possible.
        /// </summary>
        private void ExpandMacro(CilInstruction instruction)
        {
            switch (instruction.OpCode.Code)
            {
                case CilCode.Br_S:
                    instruction.OpCode = CilOpCodes.Br;
                    break;
                case CilCode.Leave_S:
                    instruction.OpCode = CilOpCodes.Leave;
                    break;
                case CilCode.Brfalse_S:
                    instruction.OpCode = CilOpCodes.Brfalse;
                    break;
                case CilCode.Brtrue_S:
                    instruction.OpCode = CilOpCodes.Brtrue;
                    break;
                case CilCode.Beq_S:
                    instruction.OpCode = CilOpCodes.Beq;
                    break;
                case CilCode.Bge_S:
                    instruction.OpCode = CilOpCodes.Bge;
                    break;
                case CilCode.Bge_Un_S:
                    instruction.OpCode = CilOpCodes.Bge_Un;
                    break;
                case CilCode.Bgt_S:
                    instruction.OpCode = CilOpCodes.Bgt;
                    break;
                case CilCode.Bgt_Un_S:
                    instruction.OpCode = CilOpCodes.Bgt_Un;
                    break;
                case CilCode.Ble_S:
                    instruction.OpCode = CilOpCodes.Ble;
                    break;
                case CilCode.Ble_Un_S:
                    instruction.OpCode = CilOpCodes.Ble_Un;
                    break;
                case CilCode.Blt_S:
                    instruction.OpCode = CilOpCodes.Blt;
                    break;
                case CilCode.Blt_Un_S:
                    instruction.OpCode = CilOpCodes.Blt_Un;
                    break;
                case CilCode.Bne_Un_S:
                    instruction.OpCode = CilOpCodes.Bne_Un;
                    break;

                case CilCode.Ldloc_S:
                    instruction.OpCode = CilOpCodes.Ldloc;
                    break;

                case CilCode.Ldloca_S:
                    instruction.OpCode = CilOpCodes.Ldloca;
                    break;

                case CilCode.Ldloc_0:
                case CilCode.Ldloc_1:
                case CilCode.Ldloc_2:
                case CilCode.Ldloc_3:
                    instruction.Operand = ((IOperandResolver) Owner).ResolveVariable(instruction.OpCode.Name[instruction.OpCode.Name.Length - 1] - 48);
                    instruction.OpCode = CilOpCodes.Ldloc;
                    break;

                case CilCode.Stloc_S:
                    instruction.OpCode = CilOpCodes.Stloc;
                    break;

                case CilCode.Stloc_0:
                case CilCode.Stloc_1:
                case CilCode.Stloc_2:
                case CilCode.Stloc_3:
                    instruction.Operand = ((IOperandResolver) Owner).ResolveVariable(instruction.OpCode.Name[instruction.OpCode.Name.Length - 1] - 48);
                    instruction.OpCode = CilOpCodes.Stloc;
                    break;

                case CilCode.Ldarg_S:
                    instruction.OpCode = CilOpCodes.Ldarg;
                    break;

                case CilCode.Ldarga_S:
                    instruction.OpCode = CilOpCodes.Ldarga;
                    break;

                case CilCode.Ldarg_0:
                case CilCode.Ldarg_1:
                case CilCode.Ldarg_2:
                case CilCode.Ldarg_3:
                    instruction.Operand = ((IOperandResolver) Owner).ResolveParameter(instruction.OpCode.Name[instruction.OpCode.Name.Length - 1] - 48);
                    instruction.OpCode = CilOpCodes.Ldarg;
                    break;

                case CilCode.Starg_S:
                    instruction.OpCode = CilOpCodes.Starg;
                    break;

                case CilCode.Ldc_I4_0:
                case CilCode.Ldc_I4_1:
                case CilCode.Ldc_I4_2:
                case CilCode.Ldc_I4_3:
                case CilCode.Ldc_I4_4:
                case CilCode.Ldc_I4_5:
                case CilCode.Ldc_I4_6:
                case CilCode.Ldc_I4_7:
                case CilCode.Ldc_I4_8:
                    instruction.Operand = instruction.OpCode.Name[instruction.OpCode.Name.Length - 1] - 48;
                    instruction.OpCode = CilOpCodes.Ldc_I4;
                    break;
                
                case CilCode.Ldc_I4_S:
                    instruction.OpCode = CilOpCodes.Ldc_I4;
                    instruction.Operand = Convert.ToInt32(instruction.Operand);
                    break;
                
                case CilCode.Ldc_I4_M1:
                    instruction.OpCode = CilOpCodes.Ldc_I4;
                    instruction.Operand = -1;
                    break;
            }
        }

        /// <summary>
        /// Optimizes the CIL instructions by shrinking instructions to macro instructions wherever possible.
        /// This method does the opposite of <see cref="ExpandMacros"/>.
        /// </summary>
        public void OptimizeMacros()
        {
            CalculateOffsets();
            foreach (var instruction in _items)
                OptimizeMacro(instruction);
            CalculateOffsets();
        }

        /// <summary>
        /// Tries to optimize the given instruction.
        /// </summary>
        /// <param name="instruction">The instruction to optimize.</param>
        private void OptimizeMacro(CilInstruction instruction)
        {
            switch (instruction.OpCode.OperandType)
            {
                case CilOperandType.InlineBrTarget:
                    TryOptimizeBranch(instruction);
                    break;
                case CilOperandType.InlineVar:
                    TryOptimizeVariable(instruction);
                    break;
                case CilOperandType.InlineArgument:
                    TryOptimizeArgument(instruction);
                    break;
            }

            if (instruction.OpCode.Code == CilCode.Ldc_I4)
                TryOptimizeLdc(instruction);
        }

        /// <summary>
        /// Tries to optimize a branch instruction to their short form whenever possible.
        /// </summary>
        /// <param name="instruction">The branch instruction.</param>
        private void TryOptimizeBranch(CilInstruction instruction)
        {
            if (!(instruction.Operand is CilInstruction operand))
                return;
            
            int relativeOperand = operand.Offset - (instruction.Offset + 2);
            if (relativeOperand < sbyte.MinValue || relativeOperand > sbyte.MaxValue)
                return;
            
            switch (instruction.OpCode.Code)
            {
                case CilCode.Br:
                    instruction.OpCode = CilOpCodes.Br_S;
                    break;
                case CilCode.Leave:
                    instruction.OpCode = CilOpCodes.Leave_S;
                    break;
                case CilCode.Brfalse:
                    instruction.OpCode = CilOpCodes.Brfalse_S;
                    break;
                case CilCode.Brtrue:
                    instruction.OpCode = CilOpCodes.Brtrue_S;
                    break;
                case CilCode.Beq:
                    instruction.OpCode = CilOpCodes.Beq_S;
                    break;
                case CilCode.Bge:
                    instruction.OpCode = CilOpCodes.Bge_S;
                    break;
                case CilCode.Bge_Un:
                    instruction.OpCode = CilOpCodes.Bge_Un_S;
                    break;
                case CilCode.Bgt:
                    instruction.OpCode = CilOpCodes.Bgt_S;
                    break;
                case CilCode.Bgt_Un:
                    instruction.OpCode = CilOpCodes.Bgt_Un_S;
                    break;
                case CilCode.Ble:
                    instruction.OpCode = CilOpCodes.Ble_S;
                    break;
                case CilCode.Ble_Un:
                    instruction.OpCode = CilOpCodes.Ble_Un_S;
                    break;
                case CilCode.Blt:
                    instruction.OpCode = CilOpCodes.Blt_S;
                    break;
                case CilCode.Blt_Un:
                    instruction.OpCode = CilOpCodes.Blt_Un_S;
                    break;
                case CilCode.Bne_Un:
                    instruction.OpCode = CilOpCodes.Bne_Un_S;
                    break;
            }
        }

        /// <summary>
        /// Tries to optimize an instruction referencing a variable.
        /// </summary>
        /// <param name="instruction">The instruction to optimize.</param>
        private void TryOptimizeVariable(CilInstruction instruction)
        {
            var variable = instruction.Operand as VariableSignature;
            if (!(Owner.Signature?.Signature is LocalVariableSignature localVarSig) || variable == null)
                return;
            
            int index = localVarSig.Variables.IndexOf(variable);
            if (index < 0 || index > byte.MaxValue)
                return;

            switch (instruction.OpCode.Code)
            {
                case CilCode.Ldloc:
                    if (index <= 3)
                    {
                        instruction.OpCode = CilOpCodes.SingleByteOpCodes[CilOpCodes.Ldloc_0.Op2 + index];
                        instruction.Operand = null;
                    }
                    else
                    {
                        instruction.OpCode = CilOpCodes.Ldloc_S;
                    }
                    break;
                case CilCode.Ldloca:
                    instruction.OpCode = CilOpCodes.Ldloca_S;
                    break;
                case CilCode.Stloc:
                    if (index <= 3)
                    {
                        instruction.OpCode = CilOpCodes.SingleByteOpCodes[CilOpCodes.Stloc_0.Op2 + index];
                        instruction.Operand = null;
                    }
                    else
                    {
                        instruction.OpCode = CilOpCodes.Stloc_S;
                    }
                    break;
            }
        }

        /// <summary>
        /// Tries to optimize an instruction referencing an argument.
        /// </summary>
        /// <param name="instruction">The instruction to optimize.</param>
        private void TryOptimizeArgument(CilInstruction instruction)
        {
            if (Owner.Method?.Signature == null || !(instruction.Operand is ParameterSignature parameter))
                return;
            
            int index = Owner.Method.Signature.Parameters.IndexOf(parameter);
            if (Owner.Method.Signature.HasThis)
                index++;

            if (index < 0 || index > byte.MaxValue)
                return;

            switch (instruction.OpCode.Code)
            {
                case CilCode.Ldarg:
                    if (index <= 3)
                    {
                        instruction.OpCode = CilOpCodes.SingleByteOpCodes[CilOpCodes.Ldarg_0.Op2 + index];
                        instruction.Operand = null;
                    }
                    else
                    {
                        instruction.OpCode = CilOpCodes.Ldarg_S;
                    }
                    break;
                case CilCode.Ldarga:
                    instruction.OpCode = CilOpCodes.Ldarga_S;
                    break;
            }
        }

        /// <summary>
        /// Tries to optimize an instruction loading a constant onto the stack.
        /// </summary>
        /// <param name="instruction">The instruction to optimize.</param>
        private void TryOptimizeLdc(CilInstruction instruction)
        {
            int value = (int) instruction.Operand;
            if (value >= -1 && value <= 8)
            {
                instruction.OpCode = CilOpCodes.SingleByteOpCodes[CilOpCodes.Ldc_I4_0.Op2 + value];
                instruction.Operand = null;
            }
            else if (value >= sbyte.MinValue && value <= sbyte.MaxValue)
            {
                instruction.OpCode = CilOpCodes.Ldc_I4_S;
                instruction.Operand = Convert.ToSByte(value);
            }
        }
    }
}