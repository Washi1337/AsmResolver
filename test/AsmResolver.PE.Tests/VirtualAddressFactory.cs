namespace AsmResolver.PE.Tests
{
    public class VirtualAddressFactory : ISegmentReferenceFactory
    {
        public static VirtualAddressFactory Instance
        {
            get;
        } = new();

        public ISegmentReference GetReferenceToRva(uint rva) => new VirtualAddress(rva);
    }
}
