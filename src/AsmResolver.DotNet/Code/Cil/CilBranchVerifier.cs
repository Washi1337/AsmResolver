using System;
using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;

namespace AsmResolver.DotNet.Code.Cil
{
    internal readonly ref struct CilBranchVerifier
    {
        private readonly CilMethodBody _body;
        private readonly List<Exception> _diagnostics;

        public CilBranchVerifier(CilMethodBody body)
        {
            _body = body ?? throw new ArgumentNullException(nameof(body));
            _diagnostics = new List<Exception>();
        }

        public void Verify()
        {
            _body.Instructions.CalculateOffsets();

            foreach (var instruction in _body.Instructions)
            {
                switch (instruction.OpCode.OperandType)
                {
                    case CilOperandType.InlineBrTarget:
                    case CilOperandType.ShortInlineBrTarget:
                        VerifyLabel(instruction.Offset, (ICilLabel) instruction.Operand);
                        break;

                    case CilOperandType.InlineSwitch:
                        var targets = (IList<ICilLabel>) instruction.Operand;
                        for (int i = 0; i < targets.Count; i++)
                            VerifyLabel(instruction.Offset, targets[i]);
                        break;
                }
            }

            switch (_diagnostics.Count)
            {
                case 0:
                    break;
                case 1:
                    throw _diagnostics[0];
                default:
                    throw new AggregateException("Method body contains multiple invalid branch targets.", _diagnostics);
            }
        }

        private void VerifyLabel(int currentOffset, ICilLabel label)
        {
            if (label is CilInstructionLabel {Instruction: { } instruction})
            {
                if (ReferenceEquals(_body.Instructions.GetByOffset(instruction.Offset), instruction))
                {
                    _diagnostics.Add(new InvalidCilInstructionException(
                        $"IL_{currentOffset:X4} references an instruction that is referencing an instruction that is not present in the method body."));
                }
            }
            else if (_body.Instructions.GetIndexByOffset(label.Offset) == -1)
            {
                _diagnostics.Add(new InvalidCilInstructionException(
                    $"IL_{currentOffset:X4} references an instruction that is referencing an offset that is not present in the method body."));
            }
        }

    }
}
