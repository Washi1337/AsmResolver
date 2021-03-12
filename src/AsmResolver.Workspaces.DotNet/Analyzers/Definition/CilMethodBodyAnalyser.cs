using System;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Cil;

namespace AsmResolver.Workspaces.DotNet.Analyzers.Definition
{
    /// <summary>
    /// Analyzes a <see cref="CilMethodBody"/> for its definitions
    /// </summary>
    public class CilMethodBodyAnalyser : ObjectAnalyzer<CilMethodBody>
    {
        /// <inheritdoc />
        public override void Analyze(AnalysisContext context, CilMethodBody subject)
        {
            for (int i = 0; i < subject.Instructions.Count; i++)
            {
                var instruction = subject.Instructions[i];
                var opType = instruction.Operand.GetType();
                switch (instruction.OpCode.OperandType)
                {
                    case CilOperandType.InlineField:
                        if (instruction.Operand is IFieldDescriptor field
                            && context.HasAnalyzers(opType))
                        {
                            context.SchedulaForAnalysis(field);
                        }
                        break;
                    case CilOperandType.InlineMethod:
                        if (instruction.Operand is IMethodDescriptor method
                            && context.HasAnalyzers(opType))
                        {
                            context.SchedulaForAnalysis(method);
                        }
                        break;
                    case CilOperandType.InlineSig:
                        if (instruction.Operand is StandAloneSignature signature
                            && context.HasAnalyzers(typeof(StandAloneSignature)))
                        {
                            context.SchedulaForAnalysis(signature);
                            if (context.HasAnalyzers(signature.Signature.GetType()))
                            {
                                context.SchedulaForAnalysis(signature.Signature);
                            }
                        }
                        break;
                    case CilOperandType.InlineTok:
                        if (instruction.Operand is IMetadataMember member
                            && context.HasAnalyzers(member.GetType()))
                        {
                            context.SchedulaForAnalysis(member);
                        }
                        break;
                    case CilOperandType.InlineType:
                        if (instruction.Operand is ITypeDefOrRef type
                            && context.HasAnalyzers(opType))
                        {
                            context.SchedulaForAnalysis(type);
                        }
                        break;
                }
            }
        }
    }
}
