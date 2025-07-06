using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using AsmResolver.PE.DotNet.Cil;

namespace AsmResolver.DotNet.Code.Cil
{
    internal readonly ref struct CilMaxStackCalculator
    {
        private readonly CilMethodBody _body;
        private readonly Stack<StackState> _agenda;
        private readonly int?[] _recordedStackSizes;
        private readonly Dictionary<int, List<CilExceptionHandler>>? _handlers;

        public CilMaxStackCalculator(CilMethodBody body)
        {
            _body = body ?? throw new ArgumentNullException(nameof(body));

            if (_body.Instructions.Count <= 0)
            {
                _agenda = null!;
                _recordedStackSizes = null!;
            }
            else
            {
                _agenda = new Stack<StackState>();
                _recordedStackSizes = new int?[_body.Instructions.Count];

                if (body.ExceptionHandlers.Count > 0)
                {
                    _handlers = new Dictionary<int, List<CilExceptionHandler>>(body.ExceptionHandlers.Count);
                    foreach (var handler in body.ExceptionHandlers)
                    {
                        if (handler.TryStart is not { Offset: int startOffset })
                            continue;

                        if (!_handlers.TryGetValue(startOffset, out var list))
                            _handlers.Add(startOffset, list = new List<CilExceptionHandler>());

                        list.Add(handler);
                    }
                }
            }
        }

        public int Compute()
        {
            if (_body.Instructions.Count == 0)
                return 0;

            int result = 0;

            // Schedule offset 0.
            _agenda.Push(new StackState(0, 0));

            while (_agenda.Count > 0)
            {
                var currentState = _agenda.Pop();

                // Check if we passed the end of the method body. This only happens if the CIL code is invalid.
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
                    {
                        throw new StackImbalanceException(_body, _body.Instructions[currentState.InstructionIndex].Offset);
                    }
                }
                else
                {
                    // Mark instruction as visited and store current state.
                    _recordedStackSizes[currentState.InstructionIndex] = currentState.StackSize;

                    // Schedule successors of current instruction.
                    ScheduleNaturalSuccessors(in currentState);
                    ScheduleExceptionalSuccessors(in currentState);
                }

                // Maintain largest found stack size.
                if (currentState.StackSize > result)
                    result = currentState.StackSize;
            }

            return result;
        }

        private void ScheduleNaturalSuccessors(in StackState currentState)
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
                        switch (instruction.Operand)
                        {
                            case ICilLabel label:
                                ScheduleLabel(currentState.InstructionIndex, label, nextStackSize);
                                break;

                            case sbyte delta:
                                ScheduleDelta(currentState.InstructionIndex, delta, nextStackSize);
                                break;

                            case int delta:
                                ScheduleDelta(currentState.InstructionIndex, delta, nextStackSize);
                                break;

                            default:
                                throw new NotSupportedException(
                                    $"Invalid or unsupported operand type at offset IL_{instruction.Offset:X4}.");
                        }
                        break;

                    case CilFlowControl.ConditionalBranch when instruction.OpCode.Code == CilCode.Switch:
                        // Schedule all switch targets for processing.
                        var targets = (IList<ICilLabel>) instruction.Operand!;
                        for (int i = 0; i < targets.Count; i++)
                            ScheduleLabel(currentState.InstructionIndex, targets[i], nextStackSize);

                        // Schedule default case (= fallthrough instruction).
                        ScheduleNext(currentState.InstructionIndex, nextStackSize);
                        break;

                    case CilFlowControl.ConditionalBranch:
                        // Schedule branch target.
                        ScheduleLabel(currentState.InstructionIndex, (ICilLabel) instruction.Operand!, nextStackSize);

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

        private void ScheduleExceptionalSuccessors(in StackState currentState)
        {
            var instruction = _body.Instructions[currentState.InstructionIndex];

            // Did we just enter at least one exception handler try block?
            if (_handlers is null || !_handlers.TryGetValue(instruction.Offset, out var handlers))
                return;

            // ECMA-335 Section I.12.4.2.8.1 prohibits entering try blocks with a non-zero stack size.
            if (currentState.StackSize != 0)
                throw new StackImbalanceException(_body, instruction.Offset);

            // Schedule handler starts.
            foreach (var handler in handlers)
            {
                // Determine stack size at the start of the handler block.
                int stackDelta = handler.HandlerType switch
                {
                    CilExceptionHandlerType.Exception => 1,
                    CilExceptionHandlerType.Filter => 1,
                    CilExceptionHandlerType.Finally => 0,
                    CilExceptionHandlerType.Fault => 0,
                    _ => throw new ArgumentOutOfRangeException(nameof(handler.HandlerType))
                };

                if (handler.HandlerStart is { } handlerStart)
                    ScheduleLabel(currentState.InstructionIndex, handlerStart, stackDelta);
                if (handler.FilterStart is { } filterStart)
                    ScheduleLabel(currentState.InstructionIndex, filterStart, 1);
            }
        }

        private void ScheduleLabel(int currentIndex, ICilLabel label, int nextStackSize)
        {
            int nextIndex = _body.Instructions.GetIndexByOffset(label.Offset);
            ScheduleIndex(currentIndex, nextIndex, label.Offset, nextStackSize);
        }

        private void ScheduleDelta(int currentIndex, int offsetDelta, int nextStackSize)
        {
            int nextOffset = _body.Instructions[currentIndex].Offset + offsetDelta;
            int nextIndex = _body.Instructions.GetIndexByOffset(nextOffset);
            ScheduleIndex(currentIndex, nextIndex, nextOffset, nextStackSize);
        }

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
