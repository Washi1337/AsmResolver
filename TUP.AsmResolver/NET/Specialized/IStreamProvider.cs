using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public interface IStreamProvider : IMetaDataMember
    {
        Stream Stream { get; }
    }
}
