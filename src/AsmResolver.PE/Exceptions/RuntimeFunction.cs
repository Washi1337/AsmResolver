namespace AsmResolver.PE.Exceptions
{
    public readonly struct RuntimeFunction
    {
        public ISegmentReference Begin
        {
            get;
        }

        public ISegmentReference End
        {
            get;
        }
    }
}
