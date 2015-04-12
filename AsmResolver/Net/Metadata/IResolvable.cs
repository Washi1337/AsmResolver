using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsmResolver.Net.Metadata
{
    public interface IResolvable
    {
        IMetadataMember Resolve();
    }
}
