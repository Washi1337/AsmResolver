namespace AsmResolver.PE.Tests
{
    public class VirtualAddressResolver : ISegmentReferenceResolver
    {
        static VirtualAddressResolver()
        {
            Instance = new VirtualAddressResolver();
        }
        
        public static ISegmentReferenceResolver Instance
        {
            get;
        }

        public ISegmentReference GetReferenceToRva(uint rva) => new VirtualAddress(rva);
    }
}