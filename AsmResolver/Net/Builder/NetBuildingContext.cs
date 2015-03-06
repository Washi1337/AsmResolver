using AsmResolver.Builder;

namespace AsmResolver.Net.Builder
{
    public class NetBuildingContext : BuildingContext
    {
        public NetBuildingContext(NetAssemblyBuilder builder)
            : base(builder)
        {
        }

        public new NetAssemblyBuilder Builder
        {
            get { return (NetAssemblyBuilder)base.Builder; }
        }

        public TBuffer GetStreamBuffer<TBuffer>() where TBuffer : FileSegment
        {
            return Builder.TextBuilder.Metadata.GetStreamBuffer<TBuffer>();
        }
    }

    
}
