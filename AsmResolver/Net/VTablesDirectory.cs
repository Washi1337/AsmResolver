using System.Collections.Generic;
using System.Linq;

namespace AsmResolver.Net
{
    public class VTablesDirectory : FileSegment
    {
        public static VTablesDirectory FromReadingContext(ReadingContext readingContext)
        {
            return new VTablesDirectory()
            {
                _readingContext = readingContext
            };
        }

        private ReadingContext _readingContext;
        private List<VTableHeader> _tableHeaders;
         
        public IList<VTableHeader> VTableHeaders
        {
            get
            {
                if (_tableHeaders != null)
                    return _tableHeaders;
                _tableHeaders = new List<VTableHeader>();
                if (_readingContext != null)
                {
                    while (_readingContext.Reader.Position < _readingContext.Reader.StartPosition + _readingContext.Reader.Length)
                    {
                        _tableHeaders.Add(VTableHeader.FromReadingContext(_readingContext));
                    }
                }

                return _tableHeaders;
            }
        }
        
        public override uint GetPhysicalLength()
        {
            return (uint) VTableHeaders.Sum(x => x.GetPhysicalLength());
        }

        public override void Write(WritingContext context)
        {
            foreach (var table in VTableHeaders)
                table.Write(context);
        }
    }
}
