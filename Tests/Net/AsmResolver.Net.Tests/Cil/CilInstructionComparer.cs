using System;
using System.Collections.Generic;
using AsmResolver.Net;
using AsmResolver.Net.Cil;
using AsmResolver.Net.Cts;

namespace AsmResolver.Tests.Net.Cil
{
    public class CilInstructionComparer : IEqualityComparer<CilInstruction>
    {
        private readonly SignatureComparer _comparer = new SignatureComparer();
        
        public bool Equals(CilInstruction x, CilInstruction y)
        {
            if (x == null && y == null)
                return true;
            if (x == null || y == null)
                return false;
            
            if (x.OpCode.Code != y.OpCode.Code || x.Offset != y.Offset)
                return false;

            switch (x.OpCode.OperandType)
            {
                case CilOperandType.InlineBrTarget:
                case CilOperandType.ShortInlineBrTarget:
                    return ((CilInstruction) x.Operand).Offset == ((CilInstruction) y.Operand).Offset;

                case CilOperandType.InlineSwitch:
                    using (var other = ((IEnumerable<CilInstruction>) y.Operand).GetEnumerator())
                    {
                        foreach (var target in (IEnumerable<CilInstruction>) x.Operand)
                        {
                            if (!other.MoveNext() || other.Current.Offset != target.Offset)
                                return false;
                        }
                        if (other.MoveNext())
                            return false;
                    }
                    return true;

                case CilOperandType.InlineMethod:
                case CilOperandType.InlineField:
                    return _comparer.Equals((IMemberReference) x.Operand, (IMemberReference) y.Operand);

                case CilOperandType.InlineType:
                    return _comparer.Equals((ITypeDescriptor) x.Operand, (ITypeDescriptor) y.Operand);

                case CilOperandType.InlineSig:
                    return _comparer.Equals(
                        ((StandAloneSignature) x.Operand).Signature,
                        ((StandAloneSignature) y.Operand).Signature);

                case CilOperandType.InlineTok:
                    if (x.Operand is IMemberReference)
                        return _comparer.Equals((IMemberReference)x.Operand, (IMemberReference)y.Operand);
                    if (x.Operand is ITypeDescriptor)
                        return _comparer.Equals((ITypeDescriptor)x.Operand, (ITypeDescriptor)y.Operand);
                    if (x.Operand is StandAloneSignature)
                        return _comparer.Equals(
                            ((StandAloneSignature)x.Operand).Signature,
                            ((StandAloneSignature)y.Operand).Signature);
                    return false;

                case CilOperandType.InlineString:
                    return x.Operand.Equals(y.Operand);
                    
                default:
                    return x.Operand == y.Operand;
            }
        }

        public int GetHashCode(CilInstruction obj)
        {
            throw new NotImplementedException();
        }
    }
}
