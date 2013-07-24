using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUP.AsmResolver.NET.Specialized
{
    public class MethodPtr : MetaDataMember
    {
        MethodDefinition _reference;

        public MethodPtr(MetaDataRow row)
            : base(row)
        {
        }

        public MethodDefinition Reference
        {
            get
            {
                if (_reference == null)
                    _netheader.TablesHeap.GetTable(MetaDataTableType.Method).TryGetMember(Convert.ToInt32(MetaDataRow.Parts[0]), out _reference);
                return _reference;
            }
        }

        public override void ClearCache()
        {
            _reference = null;
        }

        public override void LoadCache()
        {
            _reference = Reference;
        }
    }
}
