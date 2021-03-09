using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AsmResolver.PE.DotNet.Cil;

namespace AsmResolver.DotNet.Code.Cil
{
    internal readonly ref struct CilMaxStackCalculator
    {
        private readonly CilMethodBody _body;
        private readonly Stack<StackState> _agenda;
        private readonly int?[] _recordedStackSizes;

        public CilMaxStackCalculator(CilMethodBody body)
        {
            _body = body ?? throw new ArgumentNullException(nameof(body));
            _agenda = new Stack<StackState>();
            _recordedStackSizes = new int?[_body.Instructions.Count];
        }

        public int Compute(bool calculateOffsets)
        {
            if (_body.Instructions.Count == 0)
                return 0;

            if (calculateOffsets)
                _body.Instructions.CalculateOffsets();

            int result = 0;

            // Add entry points to agenda.
            ScheduleEntryPoints();

            while (_agenda.Count > 0)
            {
                var currentState = _agenda.Pop();

                // Check if we got passed the end of the method body. This only happens if the CIL code is invalid.
                if (currentState.InstructionIndex >= _body.Instructions.Count)
                {
                    var last = _body.Instructions[_body.Instructions.Count - 1];
                    throw new StackImbalanceException(_body, last.Offset + last.Size);
                }

                int? recordedStackSize = _recordedStackSizes[currentState.InstructionIndex];
                if (recordedStackSize.HasValue)
                {
                    // Check if previously visited state is consistent with current observation.
                    if (recordedStackSize.Value != currentState.StackSize)
                        throw new StackImbalanceException(_body, _body.Instructions[currentState.InstructionIndex].Offset);
                }
                else
                {
                    // Mark instruction as visited and store current state.
                    _recordedStackSizes[currentState.InstructionIndex] = currentState.StackSize;

                    // Schedule successors of current instruction.
                    ScheduleSuccessors(currentState);
                }

                // Maintain largest found stack size.
                if (currentState.StackSize > result)
                    result = currentState.StackSize;
            }

            return result;
        }

        private void ScheduleEntryPoints()
        {
            // Schedule offset 0.
            _agenda.Push(new StackState(0, 0));

            // Handler blocks are not referenced explicitly by instructions.
            // Therefore we need to schedule them explicitly as well.
            var instructions = _body.Instructions;

            for (int i = 0; i < _body.ExceptionHandlers.Count; i++)
            {
                var handler = _body.ExceptionHandlers[i];

                // Determine stack size at the start of the handler block.
                int stackDelta = handler.HandlerType switch
                {
                    CilExceptionHandlerType.Exception => 1,
                    CilExceptionHandlerType.Filter => 1,
                    CilExceptionHandlerType.Finally => 0,
                    CilExceptionHandlerType.Fault => 0,
                    _ => throw new ArgumentOutOfRangeException(nameof(handler.HandlerType))
                };

                _agenda.Push(new StackState(instructions.GetIndexByOffset(handler.TryStart.Offset), 0));
                _agenda.Push(new StackState(instructions.GetIndexByOffset(handler.HandlerStart.Offset), stackDelta));

                if (handler.FilterStart is {Offset: { } offset})
                    _agenda.Push(new StackState(instructions.GetIndexByOffset(offset), 1));
            }
        }

        private void ScheduleSuccessors(in StackState currentState)
        {
            var instruction = _body.Instructions[currentState.InstructionIndex];

            // Pop values from stack.
            int popCount = instruction.GetStackPopCount(_body);
            int nextStackSize = popCount == -1 ? 0 : currentState.StackSize - popCount;
            if (nextStackSize < 0)
                throw new StackImbalanceException(_body, instruction.Offset);

            // Push values on the stack.
            nextStackSize += instruction.GetStackPushCount();

            // Add outgoing edges to agenda.
            if (instruction.OpCode.Code == CilCode.Jmp)
            {
                // jmp instructions need special treatment:
                // Upon execution of a jmp instruction, the stack must be empty.
                // Besides, jmps have no outgoing edges, even though they are classified as FlowControl.Call.
                if (nextStackSize != 0)
                    throw new StackImbalanceException(_body, instruction.Offset);
            }
            else
            {
                switch (instruction.OpCode.FlowControl)
                {
                    case CilFlowControl.Branch:
                        // Schedule branch target.
                        ScheduleLabel(instruction.Offset, (ICilLabel) instruction.Operand, nextStackSize);
                        break;

                    case CilFlowControl.ConditionalBranch when instruction.OpCode.Code == CilCode.Switch:
                        // Schedule all switch targets for processing.
                        var targets = (IList<ICilLabel>) instruction.Operand;
                        for (int i = 0; i < targets.Count; i++)
                            ScheduleLabel(instruction.Offset, targets[i], nextStackSize);

                        // Schedule default case (= fallthrough instruction).
                        ScheduleNext(currentState.InstructionIndex, nextStackSize);
                        break;

                    case CilFlowControl.ConditionalBranch:
                        // Schedule branch target.
                        ScheduleLabel(instruction.Offset, (ICilLabel) instruction.Operand, nextStackSize);

                        // Schedule fallthrough instruction.
                        ScheduleNext(currentState.InstructionIndex, nextStackSize);
                        break;

                    case CilFlowControl.Call:
                    case CilFlowControl.Break:
                    case CilFlowControl.Meta:
                    case CilFlowControl.Phi:
                    case CilFlowControl.Next:
                        // Schedule fallthrough instruction.
                        ScheduleNext(currentState.InstructionIndex, nextStackSize);
                        break;

                    case CilFlowControl.Throw:
                        // Throw instructions just stop execution and clear any remaining values on stack.
                        // => no stack imbalance if too many values are pushed on the stack.
                        break;

                    case CilFlowControl.Return:
                        // Verify final stack size is correct.
                        if (nextStackSize != 0)
                            throw new StackImbalanceException(_body, instruction.Offset);
                        break;

                    default:
                        throw new NotSupportedException(
                            $"Invalid or unsupported operand type at offset IL_{instruction.Offset:X4}.");
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ScheduleLabel(int currentIndex, ICilLabel label, int nextStackSize)
        {
            int nextIndex = _body.Instructions.GetIndexByOffset(label.Offset);
            ScheduleIndex(currentIndex, nextIndex, label.Offset, nextStackSize);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ScheduleNext(int currentIndex, int nextStackSize)
        {
            var instruction = _body.Instructions[currentIndex];
            ScheduleIndex(currentIndex, currentIndex + 1, instruction.Offset + instruction.Size, nextStackSize);
        }

        private void ScheduleIndex(int currentIndex, int nextIndex, int nextOffset, int nextStackSize)
        {
            if (nextIndex < 0 && nextIndex >= _body.Instructions.Count)
            {
                var instruction = _body.Instructions[currentIndex];
                throw new InvalidProgramException(
                    $"Instruction at offset IL_{instruction.Offset:X4} transfers control to a non-existing offset IL_{nextOffset:X4}.");
            }

            _agenda.Push(new StackState(nextIndex, nextStackSize));
        }

        /// <summary>
        /// Provides information about the state of the stack at a particular point of execution in a method.
        /// </summary>
        private readonly struct StackState
        {
            /// <summary>
            /// The index of the instruction the state is associated to.
            /// </summary>
            public readonly int InstructionIndex;

            /// <summary>
            /// The number of values currently on the stack.
            /// </summary>
            public readonly int StackSize;

            public StackState(int instructionIndex, int stackSize)
            {
                InstructionIndex = instructionIndex;
                StackSize = stackSize;
            }

#if DEBUG
            public override string ToString()
            {
                return $"InstructionIndex: {InstructionIndex}, StackSize: {StackSize}";
            }
#endif
        }
    }
}
