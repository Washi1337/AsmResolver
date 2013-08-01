using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUP.AsmResolver.NET.Specialized
{
    public class FieldPtr : MetaDataMember
    {    
        private FieldDefinition _reference;

        public FieldPtr(MetaDataRow row)
            : base(row)
        {
        }

        public FieldPtr(FieldDefinition reference)
            : base(new MetaDataRow(reference.TableIndex))
        {
            this._reference = reference;
        }

        public FieldDefinition Reference
        {
            get
            {
                if (_reference == null)
                {
                    _netheader.TablesHeap.GetTable(MetaDataTableType.Field).TryGetMember(Convert.ToInt32(MetaDataRow.Parts[0]), out _reference);
                }
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
