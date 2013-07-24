using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public interface IGenericContext : IMetaDataMember
    {
        IGenericParamProvider Method { get; }
        IGenericParamProvider Type { get; }
        bool IsDefinition { get; }
    }
}
