using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class ModuleReference : MetaDataMember, IResolutionScope
    {
        private string _name = null;

        public ModuleReference(MetaDataRow row)
            : base(row)
        {
        }

        public ModuleReference(string name)
            : base(new MetaDataRow((uint)0))
        {
        }

        public virtual string Name
        {
            get
            {
                if (_name == null)
                    _netheader.StringsHeap.TryGetStringByOffset(Convert.ToUInt32(_metadatarow._parts[0]), out _name);
                return _name;
            }
        }
        public override string ToString()
        {
            return Name;
        }

        public override void ClearCache()
        {
            _name = null;
        }

        public override void LoadCache()
        {
            _name = Name;
        }
    }
}
