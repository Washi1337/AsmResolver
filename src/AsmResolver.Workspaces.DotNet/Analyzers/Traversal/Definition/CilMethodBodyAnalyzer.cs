using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;

namespace AsmResolver.Workspaces.DotNet.Analyzers.Traversal.Definition
{
    /// <summary>
    /// Analyzes a <see cref="CilMethodBody"/> for its definitions
    /// </summary>
    public class CilMethodBodyAnalyzer : ObjectAnalyzer<CilMethodBody>
    {
        /// <inheritdoc />
        protected override void Analyze(AnalysisContext context, CilMethodBody subject)
        {
            if(context.HasAnalyzers(typeof(CilExceptionHandler)))
            {
                for (int i = 0; i < subject.ExceptionHandlers.Count; i++)
                {
                    context.ScheduleForAnalysis(subject.ExceptionHandlers[i]);
                }
            }

            if (context.HasAnalyzers(typeof(CilLocalVariable)))
            {
                for (int i = 0; i < subject.LocalVariables.Count; i++)
                {
                    context.ScheduleForAnalysis(subject.LocalVariables[i]);
                }
            }

            AnalyzeInstructions(context, subject);
        }

        private static void AnalyzeInstructions(AnalysisContext context, CilMethodBody subject)
        {
            for (int i = 0; i < subject.Instructions.Count; i++)
            {
                var instruction = subject.Instructions[i];
                var opType = instruction.Operand?.GetType();
                switch (instruction.OpCode.OperandType)
                {
                    case CilOperandType.InlineField:
                        if (instruction.Operand is IFieldDescriptor field
                            && context.HasAnalyzers(opType!))
                        {
                            context.ScheduleForAnalysis(field);
                        }

                        break;
                    case CilOperandType.InlineMethod:
                        if (instruction.Operand is IMethodDescriptor method
                            && context.HasAnalyzers(opType!))
                        {
                            context.ScheduleForAnalysis(method);
                        }

                        break;
                    case CilOperandType.InlineSig:
                        if (instruction.Operand is StandAloneSignature signature
                            && context.HasAnalyzers(typeof(StandAloneSignature)))
                        {
                            context.ScheduleForAnalysis(signature);
                        }
                        break;
                    case CilOperandType.InlineTok:
                        if (instruction.Operand is IMetadataMember member
                            && context.HasAnalyzers(opType!))
                        {
                            context.ScheduleForAnalysis(member);
                        }

                        break;
                    case CilOperandType.InlineType:
                        if (instruction.Operand is ITypeDefOrRef type
                            && context.HasAnalyzers(opType!))
                        {
                            context.ScheduleForAnalysis(type);
                        }

                        break;
                }
            }
        }
    }
}
