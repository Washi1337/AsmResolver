using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TUP.AsmResolver.NET.Specialized;
namespace TUP.AsmResolver.NET
{
    public class BlobSignatureReader : BinaryReader
    {
        public BlobSignatureReader(Stream input)
            : base(input)
        {
        }

        public IGenericContext GenericContext { get; set; }

    }
}
