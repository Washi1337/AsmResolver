using System.Collections.Generic;
using System.Linq;
using AsmResolver.Emit;

namespace AsmResolver.Net.Emit
{
    public class CompactNetTextContent : FileSegmentBuilder
    {
        public CompactNetTextContent(ImageNetDirectory netDirectory, IOffsetConverter converter, bool is32Bit)
        {
            ImportDirectory = new ImportDirectoryBuffer(converter, is32Bit);

            Segments.Add(ImportDirectory.AddressTables);
            Segments.Add(netDirectory);
            
        }

        public ImportDirectoryBuffer ImportDirectory
        {
            get;
            private set;
        }

        public ImageNetDirectory NetDirectory
        {
            get;
            private set;
        }

        public RvaDataSegmentTableBuffer MethodBodyTable
        {
            get;
            private set;
        }

        public RvaDataSegmentTableBuffer 
        
        
    }
}
