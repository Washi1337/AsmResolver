using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class ParamPtr : MetaDataMember
    {
        private ParameterDefinition _reference;

        public ParamPtr(MetaDataRow row)
            : base(row)
        {
        }

        public ParamPtr(ParameterDefinition reference)
            : base(new MetaDataRow(reference.TableIndex))
        {
            this._reference = reference;
        }

        public ParameterDefinition Reference
        {
            get
            {
                if (_reference == null)
                {
                    _netheader.TablesHeap.GetTable(MetaDataTableType.Param).TryGetMember(Convert.ToInt32(MetaDataRow.Parts[0]), out _reference);
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
