using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUP.AsmResolver.NET.Specialized
{
    public interface IResolutionScope
    {
        string Name { get; }
        uint MetaDataToken { get; }
        MetaDataRow MetaDataRow { get; }
    }
}
