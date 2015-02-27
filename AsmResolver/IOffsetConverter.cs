using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsmResolver
{
    public interface IOffsetConverter
    {
        long RvaToFileOffset(long rva);
        long FileOffsetToRva(long fileOffset);
    }
}
