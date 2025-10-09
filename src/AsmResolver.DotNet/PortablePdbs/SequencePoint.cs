using System.Diagnostics;
using AsmResolver.PE.DotNet.Cil;

namespace AsmResolver.DotNet.PortablePdbs
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class SequencePoint
    {
        public const int HiddenLine = 0xfeefee;

        public Document? Document { get; set; }
        public ICilLabel? Instruction { get; set; }
        public int StartLine { get; set; }
        public int EndLine { get; set; }
        public int StartColumn { get; set; }
        public int EndColumn { get; set; }

        public bool IsHidden => StartLine == HiddenLine && EndLine == HiddenLine && StartColumn == 0 && EndColumn == 0;

        private string DebuggerDisplay => IsHidden ? "Hidden" : $"{StartLine}:{StartColumn} - {EndLine}:{EndColumn}";

        public static SequencePoint CreateHidden() => new()
        {
            StartLine = HiddenLine,
            EndLine = HiddenLine,
            StartColumn = 0,
            EndColumn = 0,
        };
    }
}
