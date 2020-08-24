namespace AsmResolver.PE.Debug
{
    public interface IDebugDataSegment : ISegment
    {
        DebugDataType Type
        {
            get;
        }
    }
}