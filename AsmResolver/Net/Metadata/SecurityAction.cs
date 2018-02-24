namespace AsmResolver.Net.Metadata
{
    public enum SecurityAction : ushort
    {
        Demand = 2,
        Assert = 3,
        Deny = 4,
        PermitOnly = 5,
        LinkDemand = 6,
        InheritanceDemand = 7,
        RequestMinimum = 8,
        RequestOptional = 9,
        RequestRefuse = 10,
    }
}