namespace AsmResolver
{
    public enum BaseRelocationType : byte
    {
        Absolute,
        High,
        Low,
        HighLow,
        HighAdj,
        MipsJmpAddr,
        ArmMov32A,
        Reserved,
        ArmMov32T,
        MipsJmpAddr16,
        Dir64,
    }
}