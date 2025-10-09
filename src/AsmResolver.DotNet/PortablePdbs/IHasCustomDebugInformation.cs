using System.Collections.Generic;
using AsmResolver.DotNet.PortablePdbs.CustomRecords;

namespace AsmResolver.DotNet.PortablePdbs
{
    public interface IHasCustomDebugInformation : IMetadataMember
    {
        IList<CustomDebugInformation> CustomDebugInformations
        {
            get;
        }
    }
}
