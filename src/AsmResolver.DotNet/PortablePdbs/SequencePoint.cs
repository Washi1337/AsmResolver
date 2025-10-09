namespace AsmResolver.DotNet.PortablePdbs
{
    public class SequencePoint
    {
        public const int HiddenLine = 0xfeefee;

        public Document? Document { get; set; }
        public int Offset { get; set; }
        public int StartLine { get; set; }
        public int EndLine { get; set; }
        public int StartColumn { get; set; }
        public int EndColumn { get; set; }

        public bool IsHidden => StartLine == HiddenLine && EndLine == HiddenLine && StartColumn == 0 && EndColumn == 0;
    }
}
