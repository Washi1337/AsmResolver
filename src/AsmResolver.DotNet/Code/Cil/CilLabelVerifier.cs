using System;
using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;

namespace AsmResolver.DotNet.Code.Cil
{
    internal struct CilLabelVerifier
    {
        private readonly CilMethodBody _body;
        private List<Exception>? _diagnostics;
        private string? _cachedName;

        public CilLabelVerifier(CilMethodBody body)
        {
            _body = body ?? throw new ArgumentNullException(nameof(body));
            _diagnostics = null;
            _cachedName = null;
        }

        public void Verify()
        {
            VerifyInstructions();
            VerifyExceptionHandlers();

            if (_diagnostics is null)
                return;

            switch (_diagnostics.Count)
            {
                case 0:
                    break;
                case 1:
                    throw _diagnostics[0];
                default:
                    throw new AggregateException($"Method body of {_cachedName} contains multiple invalid labels.", _diagnostics);
            }
        }

        private void VerifyInstructions()
        {
            foreach (var instruction in _body.Instructions)
            {
                switch (instruction.OpCode.OperandType)
                {
                    case CilOperandType.InlineBrTarget:
                    case CilOperandType.ShortInlineBrTarget:
                        VerifyBranchLabel(instruction.Offset, instruction.Operand as ICilLabel);
                        break;

                    case CilOperandType.InlineSwitch:
                        if (instruction.Operand is not IList<ICilLabel> targets)
                        {
                            AddDiagnostic($"Switch table of IL_{instruction.Offset:X4} is null.");
                        }
                        else
                        {
                            for (int i = 0; i < targets.Count; i++)
                                VerifyBranchLabel(instruction.Offset, targets[i]);
                        }
                        break;
                }
            }
        }

        private void VerifyExceptionHandlers()
        {
            for (int i = 0; i < _body.ExceptionHandlers.Count; i++)
            {
                var handler = _body.ExceptionHandlers[i];
                VerifyHandlerLabel(i, "Try Start", handler.TryStart);
                VerifyHandlerLabel(i, "Try End", handler.TryEnd);
                VerifyHandlerLabel(i, "Handler Start", handler.HandlerStart);
                VerifyHandlerLabel(i, "Handler End", handler.HandlerEnd);

                if (handler.HandlerType == CilExceptionHandlerType.Filter)
                    VerifyHandlerLabel(i, "Filter Start", handler.FilterStart);
            }
        }

        private void VerifyBranchLabel(int offset, ICilLabel? label)
        {
            switch (label)
            {
                case null:
                    AddDiagnostic($"Branch target of IL_{offset:X4} is null.");
                    break;

                case CilInstructionLabel {Instruction: { } instruction}:
                    if (!IsPresentInBody(instruction))
                        AddDiagnostic($"IL_{offset:X4} references an instruction that is not present in the method body.");
                    break;

                default:
                    if (!IsPresentInBody(label.Offset))
                        AddDiagnostic($"IL_{offset:X4} references an offset that is not present in the method body.");
                    break;
            }
        }

        private void VerifyHandlerLabel(int handlerIndex, string labelName, ICilLabel? label)
        {
            switch (label)
            {
                case null:
                    AddDiagnostic($"{labelName} of exception handler {handlerIndex.ToString()} is null.");
                    break;

                case CilInstructionLabel {Instruction: { } instruction}:
                    if (!IsPresentInBody(instruction))
                        AddDiagnostic($"{labelName} of exception handler {handlerIndex.ToString()} references an instruction that is not present in the method body.");
                    break;

                default:
                    if (!IsPresentInBody(label.Offset))
                        AddDiagnostic($"{labelName} of exception handler {handlerIndex.ToString()} references an offset that is not present in the method body.");
                    break;
            }
        }

        private bool IsPresentInBody(CilInstruction instruction) =>
            ReferenceEquals(_body.Instructions.GetByOffset(instruction.Offset), instruction);

        private bool IsPresentInBody(int offset) =>
            _body.Instructions.GetIndexByOffset(offset) >= 0 || _body.Instructions.EndLabel.Offset == offset;

        private void AddDiagnostic(string message)
        {
            _diagnostics ??= new List<Exception>();
            _cachedName ??= _body.Owner.SafeToString();
            _diagnostics.Add(new InvalidCilInstructionException($"[In {_cachedName}]: {message}"));
        }

    }
}
